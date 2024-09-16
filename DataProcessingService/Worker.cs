using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

using DataProcessingService.Interfaces;

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IBatchProcessor _batchProcessor;

    public Worker(
        ILogger<Worker> logger,
        IBatchProcessor batchProcessor)
    {
        _logger = logger;
        _batchProcessor = batchProcessor;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        int[] batchSizes = { 5000, 10000, 20000 };
        int[] subBatchSizes = { 500, 1000, 2000 };
        int[] degreesOfParallelism = { 5, 10, 20 };

        foreach (var batchSize in batchSizes)
        {
            foreach (var subBatchSize in subBatchSizes)
            {
                foreach (var maxDegreeOfParallelism in degreesOfParallelism)
                {
                    _logger.LogInformation("Starting test with batchSize={BatchSize}, subBatchSize={SubBatchSize}, maxDegreeOfParallelism={MaxDegreeOfParallelism}",
                        batchSize, subBatchSize, maxDegreeOfParallelism);

                    var stopwatch = Stopwatch.StartNew();

                    _batchProcessor.UpdateConfiguration(maxDegreeOfParallelism);

                    await _batchProcessor.ProcessBatchesAsync(batchSize, subBatchSize);

                    stopwatch.Stop();

                    _logger.LogInformation("Test completed. Total time: {Elapsed}", stopwatch.Elapsed);
                    _logger.LogInformation("BatchSize: {BatchSize}, SubBatchSize: {SubBatchSize}, MaxDegreeOfParallelism: {MaxDegreeOfParallelism}, TotalTime: {TotalTime}",
                        batchSize, subBatchSize, maxDegreeOfParallelism, stopwatch.Elapsed);
                }
            }
        }
    }
}
