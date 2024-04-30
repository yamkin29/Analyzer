namespace Analyzer.Tests;

[TestFixture]
public class MergeParametersTests
{
    [Test]
    public void MergeParameters_OverrideNullValues_ReturnsMergedParameters()
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
            TimeStart = new DateTime(2022, 1, 1),
            TimeEnd = new DateTime(2023, 1, 1)
        };

        var mergedParameters = LogAnalyzer.MergeParameters(parameters, configFileParameters);

        Assert.That(mergedParameters.FileLog, Is.EqualTo("configLog.log"));
        Assert.That(mergedParameters.FileOutput, Is.EqualTo("configOutput.txt"));
        Assert.That(mergedParameters.AddressStart, Is.EqualTo("192.168.0.1"));
        Assert.That(mergedParameters.AddressMask, Is.EqualTo("255.255.255.0"));
        Assert.That(mergedParameters.TimeStart, Is.EqualTo(new DateTime(2022, 1, 1)));
        Assert.That(mergedParameters.TimeEnd, Is.EqualTo(new DateTime(2023, 1, 1)));
    }

    [Test]
    public void MergeParameters_DifferentValues_OverridesCorrectly()
    {
        var parameters = new Parameters
        {
            FileLog = "log1.txt",
            FileOutput = "output1.txt",
            AddressStart = "192.168.1.1",
            AddressMask = "255.255.0.0",
            TimeStart = new DateTime(2022, 1, 1),
            TimeEnd = new DateTime(2023, 1, 1)
        };

        var configFileParameters = new Parameters
        {
            FileLog = "configLog.log",
            FileOutput = "configOutput.txt",
            AddressStart = "192.168.0.1",
            AddressMask = "255.255.255.0",
            TimeStart = new DateTime(2022, 1, 1),
            TimeEnd = new DateTime(2023, 1, 1)
        };

        var mergedParameters = LogAnalyzer.MergeParameters(parameters, configFileParameters);

        Assert.That(mergedParameters.FileLog, Is.EqualTo("log1.txt")); // Не должно измениться
        Assert.That(mergedParameters.FileOutput, Is.EqualTo("output1.txt")); // Не должно измениться
        Assert.That(mergedParameters.AddressStart, Is.EqualTo("192.168.1.1")); // Не должно измениться
        Assert.That(mergedParameters.AddressMask, Is.EqualTo("255.255.0.0")); // Не должно измениться
    }
}