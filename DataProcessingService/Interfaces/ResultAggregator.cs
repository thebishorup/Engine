using DataProcessingService.Models;

namespace DataProcessingService.Interfaces;

public class ResultAggregator : IResultAggregator
{
    private readonly IWriteBehindCache _writeBehindCache;

    public ResultAggregator(IWriteBehindCache writeBehindCache)
    {
        _writeBehindCache = writeBehindCache;
    }

    public async Task AggregateResultAsync(RuleResult result)
    {
        var aggregatedResult = new AggregatedResult
        {
            AccountId = result.AccountId,
            IsValid = result.IsValid,
            ValidationMessage = result.ValidationMessage,
            ProcessedAt = DateTime.UtcNow
        };

        await _writeBehindCache.AddResultAsync(aggregatedResult);
    }
}
