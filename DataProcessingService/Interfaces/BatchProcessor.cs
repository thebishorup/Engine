using DataProcessingService.Models;

namespace DataProcessingService.Interfaces;

public class BatchProcessor : IBatchProcessor
{
    private readonly IDataPrefetcher _dataPrefetcher;
    private readonly IRuleEngine _ruleEngine;
    private readonly IResultAggregator _resultAggregator;
    private int _maxDegreeOfParallelism;
    private SemaphoreSlim _concurrencySemaphore;

    public BatchProcessor(
        IDataPrefetcher dataPrefetcher,
        IRuleEngine ruleEngine,
        IResultAggregator resultAggregator,
        int maxDegreeOfParallelism)
    {
        _dataPrefetcher = dataPrefetcher;
        _ruleEngine = ruleEngine;
        _resultAggregator = resultAggregator;
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
        _concurrencySemaphore = new SemaphoreSlim(maxDegreeOfParallelism);
    }

    public async Task ProcessBatchesAsync(int batchSize, int subBatchSize)
    {
        int lastAccountId = 0;

        while (true)
        {
            // Fetch accounts for the current batch
            var accounts = await _dataPrefetcher.FetchAccountsAsync(lastAccountId, batchSize);

            if (accounts.Count == 0)
                break;

            // Update lastAccountId for the next batch
            lastAccountId = accounts.Last().AccountId;

            // Prefetch related data
            var prefetchedDataList = await _dataPrefetcher.PrefetchDataAsync(accounts);

            // Split into sub-batches
            var subBatches = SplitIntoSubBatches(prefetchedDataList, subBatchSize);

            // Process sub-batches in parallel
            var tasks = subBatches.Select(async subBatch =>
            {
                await _concurrencySemaphore.WaitAsync();
                try
                {
                    // Apply rules to the sub-batch
                    var ruleResults = await _ruleEngine.ApplyRulesAsync(subBatch);

                    // Aggregate results
                    foreach (var ruleResult in ruleResults)
                    {
                        await _resultAggregator.AggregateResultAsync(ruleResult);
                    }
                }
                finally
                {
                    _concurrencySemaphore.Release();
                }
            });

            await Task.WhenAll(tasks);
        }
    }

    public void UpdateConfiguration(int maxDegreeOfParallelism)
    {
        _maxDegreeOfParallelism = maxDegreeOfParallelism;
        _concurrencySemaphore = new SemaphoreSlim(_maxDegreeOfParallelism);
    }

    private List<List<PrefetchedData>> SplitIntoSubBatches(List<PrefetchedData> dataList, int subBatchSize)
    {
        var subBatches = new List<List<PrefetchedData>>();

        for (int i = 0; i < dataList.Count; i += subBatchSize)
        {
            subBatches.Add(dataList.GetRange(i, Math.Min(subBatchSize, dataList.Count - i)));
        }

        return subBatches;
    }
}
