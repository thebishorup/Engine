using BenchmarkDotNet.Attributes;

using DataProcessingService.Interfaces;
using DataProcessingService.Models;

using Microsoft.Extensions.Caching.Memory;

public class RuleEngineBenchmarks
{
    private IRuleEngine _ruleEngine;
    private List<PrefetchedData> _dataBatch;

    [GlobalSetup]
    public void Setup()
    {
        // Initialize _ruleEngine and _dataBatch
        _ruleEngine = new RuleEngine(new MemoryCacheService(new MemoryCache(new MemoryCacheOptions())), new ConfigurationBuilder().Build());
        _dataBatch = GenerateTestData(1000); // Generate a batch of 1000 accounts
    }

    [Benchmark]
    public async Task ApplyRulesBenchmark()
    {
        await _ruleEngine.ApplyRulesAsync(_dataBatch);
    }

    private List<PrefetchedData> GenerateTestData(int count)
    {
        // Generate test data
        var dataBatch = new List<PrefetchedData>();
        for (int i = 0; i < count; i++)
        {
            dataBatch.Add(new PrefetchedData
            {
                AccountData = new AccountData { AccountId = i, AccountName = $"TestAccount_{i}" },
                //AccountDetails = new AccountDetails { AccountId = i, Balance = GetRandomBalance() }
            });
        }
        return dataBatch;
    }

    private decimal GetRandomBalance()
    {
        var random = new Random();
        return (decimal)(random.NextDouble() * 10000);
    }
}
