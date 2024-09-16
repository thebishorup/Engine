using DataProcessingService.Models;

using Microsoft.Extensions.Options;

namespace DataProcessingService.Interfaces;

public class WriteBehindCache : IWriteBehindCache
{
    private readonly List<AggregatedResult> _results = new();
    private readonly SemaphoreSlim _semaphore = new(1, 1);
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<WriteBehindCache> _logger;
    private Timer _timer;
    private readonly int _batchSize;
    private readonly int _flushIntervalSeconds;

    public WriteBehindCache(
        IServiceScopeFactory scopeFactory,
        ILogger<WriteBehindCache> logger,
        IOptions<BulkOperationsOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _batchSize = options.Value.BatchSize;
        _flushIntervalSeconds = options.Value.FlushIntervalSeconds;
        _timer = new Timer(FlushCallback, null, TimeSpan.Zero, TimeSpan.FromSeconds(_flushIntervalSeconds));
    }

    public async Task AddResultAsync(AggregatedResult result)
    {
        await _semaphore.WaitAsync();
        try
        {
            _results.Add(result);

            if (_results.Count >= _batchSize)
            {
                _timer.Change(Timeout.Infinite, Timeout.Infinite); // Stop the timer
                await FlushAsync();
                _timer.Change(TimeSpan.FromSeconds(_flushIntervalSeconds), TimeSpan.FromSeconds(_flushIntervalSeconds)); // Restart the timer
            }
        }
        finally
        {
            _semaphore.Release();
        }
    }

    private async void FlushCallback(object state)
    {
        await FlushAsync();
    }

    public async Task FlushAsync()
    {
        List<AggregatedResult> resultsToUpsert;

        await _semaphore.WaitAsync();
        try
        {
            if (!_results.Any())
            {
                return;
            }

            resultsToUpsert = new List<AggregatedResult>(_results);
            _results.Clear();
        }
        finally
        {
            _semaphore.Release();
        }

        try
        {
            //using var scope = _scopeFactory.CreateScope();
            //var dbContext = scope.ServiceProvider.GetRequiredService<YourDbContext>();

            //var bulkConfig = new BulkConfig
            //{
            //    BatchSize = _batchSize,
            //    UseTempDB = true,
            //    SetOutputIdentity = true,
            //    UpdateByProperties = new List<string> { nameof(AggregatedResult.AccountId) }
            //};

            //await dbContext.BulkInsertOrUpdateAsync(resultsToUpsert, bulkConfig);

            //_logger.LogInformation("Bulk upsert completed for {Count} records.", resultsToUpsert.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during bulk upsert.");
            // Implement retry logic or error handling as needed
        }
    }
}
