using DataProcessingService.Interfaces;
using DataProcessingService.Models;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        IConfiguration configuration = hostContext.Configuration;

        // Add DbContext (only if using EF Core for bulk upserts)
        //services.AddDbContext<YourDbContext>(options =>
        //    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        // Register services
        services.AddHostedService<Worker>();
        services.AddScoped<IDataPrefetcher, DataPrefetcher>();
        services.AddSingleton<ICacheService, MemoryCacheService>();
        services.AddScoped<IRuleEngine, RuleEngine>();
        services.AddScoped<IResultAggregator, ResultAggregator>();
        services.AddSingleton<IWriteBehindCache, WriteBehindCache>();

        // Configure BatchProcessor with parameters
        services.AddScoped<IBatchProcessor>(sp =>
        {
            var dataPrefetcher = sp.GetRequiredService<IDataPrefetcher>();
            var ruleEngine = sp.GetRequiredService<IRuleEngine>();
            var resultAggregator = sp.GetRequiredService<IResultAggregator>();
            var maxDegreeOfParallelism = 10; // Adjust based on system capabilities

            return new BatchProcessor(
                dataPrefetcher,
                ruleEngine,
                resultAggregator,
                maxDegreeOfParallelism);
        });

        // Add MemoryCache
        services.AddMemoryCache();

        // Configure options
        services.Configure<BulkOperationsOptions>(configuration.GetSection("BulkOperations"));
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddConsole();
    })
    .Build();

await host.RunAsync();
