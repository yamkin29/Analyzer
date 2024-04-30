using Microsoft.Extensions.Configuration;

namespace Analyzer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            
            IConfiguration? configuration = builder.Build();

            var analyzer = new LogAnalyzer(configuration);
            analyzer.Analyze();
        }
    }
}
