namespace Analyzer.Tests;

[TestFixture]
public class ReadLogFileTests
{
    private string _testLogFilePath;

    [SetUp]
    public void Setup()
    {
        _testLogFilePath = Path.GetTempFileName();
        File.WriteAllText(_testLogFilePath, "192.168.0.1: 2024-04-01 10:00:00\n192.168.0.2: 2024-04-01 10:00:00\n");
    }

    [TearDown]
    public void TearDown()
    {
        File.Delete(_testLogFilePath);
    }

    [Test]
    public void ReadLogFile_ValidLog_ReturnsCorrectResults()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var results = LogAnalyzer.ReadLogFile(
            _testLogFilePath,
            "255.255.255.0",
            "192.168.0.0",
            cancellationTokenSource);

        Assert.IsNotNull(results);
        Assert.That(results.Count, Is.EqualTo(2));
        Assert.That(results.ContainsKey("192.168.0.1"));
        Assert.That(results.ContainsKey("192.168.0.2"));
    }

    [Test]
    public void ReadLogFile_CancelledOperation_ReturnsNull()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel(); // Имитация отмены

        var results = LogAnalyzer.ReadLogFile(
            _testLogFilePath,
            "255.255.255.0",
            "192.168.0.0",
            cancellationTokenSource);
    }
}