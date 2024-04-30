namespace Analyzer.Tests;

[TestFixture]
public class CommandLineArgumentTests
{
    [Test]
    public void ParseCommandLineArguments_ValidArguments_ReturnsParameters()
    {
        string[] args = { "--file-log", "log.txt", "--file-output", "output.txt", "--address-start", "192.168.0.1", "--address-mask", "255.255.255.0", "--time-start", "01.01.2024", "--time-end", "01.01.2025" };
        var parameters = LogAnalyzer.ParseCommandLineArguments(args);

        Assert.IsNotNull(parameters);
        Assert.That(parameters.FileLog, Is.EqualTo("log.txt"));
        Assert.That(parameters.FileOutput, Is.EqualTo("output.txt"));
        Assert.That(parameters.AddressStart, Is.EqualTo("192.168.0.1"));
        Assert.That(parameters.AddressMask, Is.EqualTo("255.255.255.0"));
        Assert.That(parameters!.TimeStart, Is.EqualTo(new DateTime(2024, 1, 1)));
        Assert.That(parameters.TimeEnd.Date, Is.EqualTo(new DateTime(2025, 1, 1).Date));
    }

    [Test]
    public void ParseCommandLineArguments_InvalidArgumentCount_ReturnsNull()
    {
        string[] args = { "--file-log", "log.txt", "--file-output" }; // Неполный набор аргументов
        var parameters = LogAnalyzer.ParseCommandLineArguments(args);

        Assert.IsNull(parameters);
    }

    [Test]
    public void ParseCommandLineArguments_UnknownParameter_ReturnsNull()
    {
        string[] args = { "--file-log", "log.txt", "--unknown-param", "value" };
        var parameters = LogAnalyzer.ParseCommandLineArguments(args);

        Assert.IsNull(parameters);
    }
}