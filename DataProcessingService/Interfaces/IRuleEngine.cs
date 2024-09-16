using DataProcessingService.Models;
namespace DataProcessingService.Interfaces;

public interface IRuleEngine
{
    Task<List<RuleResult>> ApplyRulesAsync(List<PrefetchedData> dataBatch);
    Task<List<Rule>> GetRulesAsync();
    void InvalidateRulesCache();
}
