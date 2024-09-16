using DataProcessingService.Models;

namespace DataProcessingService.Interfaces;

public interface IResultAggregator
{
    Task AggregateResultAsync(RuleResult result);
}
