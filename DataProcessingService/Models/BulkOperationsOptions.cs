namespace DataProcessingService.Models;

public class BulkOperationsOptions
{
    public int BatchSize { get; set; } = 1000;
    public int FlushIntervalSeconds { get; set; } = 30;
}

