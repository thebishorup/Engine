using DataProcessingService.Models;

namespace DataProcessingService.Interfaces;

public interface IWriteBehindCache
{
    Task AddResultAsync(AggregatedResult result);
    Task FlushAsync();
}
