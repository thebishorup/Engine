namespace DataProcessingService.Interfaces;

public interface IBatchProcessor
{
    Task ProcessBatchesAsync(int batchSize, int subBatchSize);
}
