namespace Analyzer.Tests
{
    [TestFixture]
    public class ProgramTests
    {
        private string _testFilePath;
        private string _testConfigFilePath;
        private string _testOutputFilePath;

        [SetUp]
        public void Setup()
        {
            _testFilePath = Path.GetTempFileName();
            _testConfigFilePath = Path.GetTempFileName();
            _testOutputFilePath = Path.GetTempFileName();
            
            File.WriteAllText(_testFilePath, "192.168.0.1: 2024-04-01 10:00:00\n192.168.0.2: 2024-04-01 10:00:00\n");
            
            File.WriteAllText(_testConfigFilePath, "{\"FileLog\": \"configLog.log\", \"FileOutput\": \"configOutput.txt\"," +
                                                   "\"AddressStart\": \"192.168.0.1\", \"AddressMask\": \"255.255.255.0\", " +
                                                   "\"TimeStart\": \"01.04.2024\", \"TimeEnd\": \"01.04.2025\"}");
        }

        [TearDown]
        public void TearDown()
        {
            File.Delete(_testFilePath);
            File.Delete(_testConfigFilePath);
            File.Delete(_testOutputFilePath);
        }

        [Test]
        public void ParseCommandLineArguments_ValidArgs_ReturnsParametersObject()
        {
            string[] args = { "--file-log", _testFilePath, "--file-output", _testOutputFilePath, "--address-start", "192.168.0.1", "--address-mask", "255.255.255.0", "--time-start", "01.04.2024", "--time-end", "01.04.2025" };
            
            var parameters = Program.ParseCommandLineArguments(args);
            
            Assert.IsNotNull(parameters);
            Assert.That(parameters?.FileLog, Is.EqualTo(_testFilePath));
            Assert.That(parameters?.FileOutput, Is.EqualTo(_testOutputFilePath));
            Assert.That(parameters?.AddressStart, Is.EqualTo("192.168.0.1"));
            Assert.That(parameters?.AddressMask, Is.EqualTo("255.255.255.0"));
            Assert.That(parameters!.TimeStart, Is.EqualTo(new DateTime(2024, 4, 1)));
            Assert.That(parameters.TimeEnd.Date, Is.EqualTo(new DateTime(2025, 4, 1).Date));

        }

        [Test]
        public void ReadConfigFile_ValidConfigFile_ReturnsParametersObject()
        {
            var parameters = Program.ReadConfigFile(_testConfigFilePath);
            
            Assert.IsNotNull(parameters);
            Assert.That(parameters?.FileLog, Is.EqualTo("configLog.log"));
            Assert.That(parameters?.FileOutput, Is.EqualTo("configOutput.txt"));
            Assert.That(parameters?.AddressStart, Is.EqualTo("192.168.0.1"));
            Assert.That(parameters?.AddressMask, Is.EqualTo("255.255.255.0"));
            Assert.That(parameters!.TimeStart, Is.EqualTo(new DateTime(2024, 1, 4)));
            Assert.That(parameters.TimeEnd.Date, Is.EqualTo(new DateTime(2025, 1, 4).Date));
        }

        [Test]
        public void MergeParameters_OverrideNullParameters_ReturnsMergedParameters()
        {
            var parameters = new Parameters
            {
                FileLog = null,
                FileOutput = null,
                AddressStart = null,
                AddressMask = null,
                TimeStart = DateTime.MinValue,
                TimeEnd = DateTime.MaxValue
            };
            var configFileParameters = new Parameters
            {
                FileLog = "configLog.log",
                FileOutput = "configOutput.txt",
                AddressStart = "192.168.0.1",
                AddressMask = "255.255.255.0",
                TimeStart = new DateTime(2024, 4, 1),
                TimeEnd = new DateTime(2025, 4, 1)
            };
            
            var mergedParameters = Program.MergeParameters(parameters, configFileParameters);
            
            Assert.That(mergedParameters.FileLog, Is.EqualTo("configLog.log"));
            Assert.That(mergedParameters.FileOutput, Is.EqualTo("configOutput.txt"));
            Assert.That(mergedParameters.AddressStart, Is.EqualTo("192.168.0.1"));
            Assert.That(mergedParameters.AddressMask, Is.EqualTo("255.255.255.0"));
            Assert.That(mergedParameters.TimeStart, Is.EqualTo(new DateTime(2024, 4, 1)));
            Assert.That(mergedParameters.TimeEnd, Is.EqualTo(new DateTime(2025, 4, 1)));
        }
    }
}
