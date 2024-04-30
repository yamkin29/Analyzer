using Microsoft.Extensions.Configuration;

namespace Analyzer.Tests;

[TestFixture]
public class ConfigFileTests
{
    private IConfiguration _configuration;
    private string _testConfigFilePath;

    [SetUp]
    public void Setup()
    {
        _testConfigFilePath = Path.GetTempFileName();
        File.WriteAllText(_testConfigFilePath, "{\"FileLog\": \"configLog.log\", \"FileOutput\": \"configOutput.txt\", \"AddressStart\": \"192.168.0.1\", \"AddressMask\": \"255.255.255.0\", \"TimeStart\": \"01.01.2022\", \"TimeEnd\": \"01.01.2023\"}");

        var builder = new ConfigurationBuilder()
            .AddJsonFile(_testConfigFilePath);

        _configuration = builder.Build();
    }

    [TearDown]
    public void TearDown()
    {
        File.Delete(_testConfigFilePath);
    }

    [Test]
    public void ReadConfigFile_ValidConfig_ReturnsParameters()
    {
        var logAnalyzer = new LogAnalyzer(_configuration);
        var configParameters = logAnalyzer.ReadConfigFile();

        Assert.IsNotNull(configParameters);
        Assert.That(configParameters.FileLog, Is.EqualTo("configLog.log"));
        Assert.That(configParameters.FileOutput, Is.EqualTo("configOutput.txt"));
        Assert.That(configParameters.AddressStart, Is.EqualTo("192.168.0.1"));
        Assert.That(configParameters.AddressMask, Is.EqualTo("255.255.255.0"));
        Assert.That(configParameters.TimeStart, Is.EqualTo(new DateTime(2022, 1, 1)));
        Assert.That(configParameters.TimeEnd, Is.EqualTo(new DateTime(2023, 1, 1)));
    }

    [Test]
    public void ReadConfigFile_FileNotFound_ReturnsNull()
    {
        var builder = new ConfigurationBuilder();
        var emptyConfiguration = builder.Build(); // Пустая конфигурация, нет файла

        var logAnalyzer = new LogAnalyzer(emptyConfiguration);
        var configParameters = logAnalyzer.ReadConfigFile();

        Assert.IsNull(configParameters);
    }

    [Test]
    public void ReadConfigFile_InvalidJson_ReturnsNull()
    {
        File.WriteAllText(_testConfigFilePath, "{invalidJson}"); // Некорректный JSON

        var logAnalyzer = new LogAnalyzer(_configuration);
        var configParameters = logAnalyzer.ReadConfigFile();

        Assert.IsNull(configParameters);
    }
}