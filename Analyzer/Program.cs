namespace Analyzer
{
    public class Program
    {
        private static readonly CancellationTokenSource CancellationTokenSource = new();
        
        static void Main(string?[] args)
        {
            var parameters = ParseCommandLineArguments(args);

            if (parameters == null)
            {
                return;
            }

            var configFileParameters = ReadConfigFile("config.json");

            if (configFileParameters != null)
            {
                parameters = MergeParameters(parameters, configFileParameters);
            }

            if (ValidateParameters(parameters) is false)
            {
                return;
            }
            
            Console.CancelKeyPress += (_, e) =>
            {
                // При нажатии Ctrl+C отменяем операцию чтения файла логов
                e.Cancel = true;
                CancellationTokenSource.Cancel();
            };

            var ipAddresses = ReadLogFile(parameters.FileLog!, parameters.AddressMask!, parameters.AddressStart!, CancellationTokenSource);

            if (ipAddresses == null)
            {
                return;
            }

            WriteResultsToFile(ipAddresses, parameters.FileOutput!);
        }

        public static Parameters? ParseCommandLineArguments(string?[] args)
        {
            if (args.Length < 4 || args.Length % 2 != 0)
            {
                Console.WriteLine("Неверное количество аргументов.");
                PrintUsageInstructions();
                return null;
            }

            var parameters = new Parameters();

            for (int i = 0; i < args.Length; i += 2)
            {
                switch (args[i])
                {
                    case "--file-log":
                        parameters.FileLog = args[i + 1];
                        break;
                    case "--file-output":
                        parameters.FileOutput = args[i + 1];
                        break;
                    case "--address-start":
                        parameters.AddressStart = args[i + 1];
                        break;
                    case "--address-mask":
                        parameters.AddressMask = args[i + 1];
                        break;
                    case "--time-start":
                        parameters.TimeStart = DateTime.ParseExact(args[i + 1]!, "dd.MM.yyyy", null);
                        break;
                    case "--time-end":
                        parameters.TimeEnd = DateTime.ParseExact(args[i + 1]!, "dd.MM.yyyy", null).AddDays(1).AddMilliseconds(-1);
                        break;
                    default:
                        Console.WriteLine($"Неизвестный параметр: {args[i]}");
                        return null;
                }
            }

            return parameters;
        }

        static void PrintUsageInstructions()
        {
            Console.WriteLine("Используйте:");
            Console.WriteLine("--file-log <путь к файлу с логами> " +
                              "--file-output <путь к файлу с результатом> " +
                              "[--address-start <нижняя граница диапазона>] (необязательный параметр) " +
                              "[--address-mask <маска подсети>] (необязательный параметр) " +
                              "--time-start <начало интервала> " +
                              "--time-end <конец интервала>");
        }

        public static Parameters? ReadConfigFile(string configFile)
        {
            if (File.Exists(configFile) is false)
            {
                Console.WriteLine("Файл конфигурации не найден.");
                return null;
            }

            try
            {
                string configContent = File.ReadAllText(configFile);
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Parameters>(configContent);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения файла конфигурации: {ex.Message}");
                return null;
            }
        }

        public static Parameters MergeParameters(Parameters parameters, Parameters configFileParameters)
        {
            parameters.FileLog ??= configFileParameters.FileLog;

            parameters.FileOutput ??= configFileParameters.FileOutput;

            parameters.AddressStart ??= configFileParameters.AddressStart;

            parameters.AddressMask ??= configFileParameters.AddressMask;

            if (parameters.TimeStart == DateTime.MinValue)
            {
                parameters.TimeStart = configFileParameters.TimeStart;
            }

            if (parameters.TimeEnd == DateTime.MaxValue)
            {
                parameters.TimeEnd = configFileParameters.TimeEnd;
            }

            return parameters;
        }

        static bool ValidateParameters(Parameters parameters)
        {
            if (parameters.FileLog == null || parameters.FileOutput == null || parameters.TimeStart >= parameters.TimeEnd ||
                (parameters.AddressMask == null && parameters.AddressStart != null))
            {
                Console.WriteLine("Неверные параметры. Убедитесь, что указаны правильные пути к файлам и временные интервалы.");
                return false;
            }

            return true;
        }

        static Dictionary<string, int>? ReadLogFile(string fileLog, string addressMask, string addressStart, CancellationTokenSource cancellationTokenSource)
        {
            var ipAddresses = new Dictionary<string, int>();

            try
            {
                using (StreamReader reader = new StreamReader(fileLog))
                {
                    string? line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        if (cancellationTokenSource.IsCancellationRequested)
                        {
                            return null;
                        }
                        
                        string[] parts = line.Split(new[] { ':' }, 2);
                        if (parts.Length != 2)
                        {
                            continue;
                        }

                        string ipAddress = parts[0];
                        string[] dateTimeParts = parts[1].Split(' ');

                        // Проверка правильности формата даты и времени
                        if (dateTimeParts.Length != 2 || !DateTime.TryParse(dateTimeParts[1], out _))
                        {
                            continue;
                        }
                        
                        // Проверка соответствия адреса диапазону
                        if (addressStart != null && addressMask != null)
                        {
                            if (IsInAddressRange(ipAddress, addressStart, addressMask) is false)
                            {
                                continue;
                            }
                        }
                        else if (addressStart != null)
                        {
                            if (ipAddress != addressStart)
                            {
                                continue;
                            }
                        }

                        ipAddresses.TryGetValue(ipAddress, out int count);
                        ipAddresses[ipAddress] = count + 1;
                    }
                }

                return ipAddresses;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка чтения файла логов: {ex.Message}");
                return null;
            }
        }

        static void WriteResultsToFile(Dictionary<string, int> results, string fileOutput)
        {
            try
            {
                using (StreamWriter writer = new StreamWriter(fileOutput))
                {
                    foreach (var kvp in results.OrderByDescending(x => x.Value))
                    {
                        writer.WriteLine($"{kvp.Key}: {kvp.Value}");
                    }
                }

                Console.WriteLine("Результаты сохранены в файл.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка записи результатов в файл: {ex.Message}");
            }
        }
        
        private static bool IsInAddressRange(string ipAddress, string? addressStart, string? addressMask)
        {
            string[] ipParts = ipAddress.Split('.');
            string[] startParts = addressStart!.Split('.');
            string[] maskParts = addressMask!.Split('.');

            for (int i = 0; i < 4; i++)
            {
                int ip = int.Parse(ipParts[i]);
                int start = int.Parse(startParts[i]);
                int mask = int.Parse(maskParts[i]);

                if ((ip & mask) != (start & mask))
                {
                    return false;
                }
            }

            return true;
        }
    }
}
