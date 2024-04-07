namespace Analyzer;

public class Parameters
{
    public string? FileLog { get; set; }
    public string? FileOutput { get; set; }
    public string? AddressStart { get; set; }
    public string? AddressMask { get; set; }
    public DateTime TimeStart { get; set; } = DateTime.MinValue;
    public DateTime TimeEnd { get; set; } = DateTime.MaxValue;
}