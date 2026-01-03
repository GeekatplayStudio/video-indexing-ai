using FootageSearch.Indexer.Services;
using FootageSearch.Core.Interfaces;

namespace FootageSearch.Indexer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IServiceProvider _serviceProvider;

    public Worker(ILogger<Worker> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Indexer Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    // Ensure DB is created before we try to write status
                    var dbContext = scope.ServiceProvider.GetRequiredService<FootageSearch.Data.VideoDbContext>();
                    await dbContext.Database.EnsureCreatedAsync();

                    var indexerService = scope.ServiceProvider.GetRequiredService<IndexerService>();
                    var jobStatusService = scope.ServiceProvider.GetRequiredService<IJobStatusService>();
                    
                    await jobStatusService.UpdateStatusAsync("Scanning", "Checking for new files...");
                    
                    var progress = new Progress<string>(async msg => 
                    {
                        _logger.LogInformation(msg);
                        await jobStatusService.UpdateStatusAsync("Indexing", msg);
                    });

                    await indexerService.ReIndexAsync(progress);
                    
                    await jobStatusService.UpdateStatusAsync("Idle", "Waiting for next scan cycle...");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during indexing cycle");
                // We need a scope to report error if the main scope failed
                try 
                {
                    using (var errScope = _serviceProvider.CreateScope())
                    {
                        var errStatusService = errScope.ServiceProvider.GetRequiredService<IJobStatusService>();
                        await errStatusService.UpdateStatusAsync("Error", ex.Message);
                    }
                }
                catch { }
            }

            // Wait 5 minutes before next scan
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
        }
    }
}