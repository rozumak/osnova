using System.Diagnostics;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osnova.ExportTasks;

namespace Osnova;

public class ExportServer : IServer
{
    private readonly ILogger<ExportServer> _logger;

    private readonly IServer _originalServer;
    private readonly IHostApplicationLifetime _hostApplicationLifetime;

    private readonly OnExportHandler _onExportHandler;

    private readonly bool _isExporting;

    private readonly CancellationTokenSource _stoppingCts = new();
    private Task? _exportTask;

    private IFeatureCollection? _emptyFeatures;

    public IFeatureCollection Features
    {
        get
        {
            if (_isExporting)
                return _emptyFeatures ??= new FeatureCollection();

            return _originalServer.Features;
        }
    }

    public ExportServer(ILoggerFactory loggerFactory, IOptions<StaticExportOptions> options, IServer originalServer,
        IHostApplicationLifetime hostApplicationLifetime, IEnumerable<IExportTask> exportTasks)
    {
        _logger = loggerFactory.CreateLogger<ExportServer>();
        _originalServer = originalServer;
        _hostApplicationLifetime = hostApplicationLifetime;

        _isExporting = options.Value.RunExportCommand;
        _onExportHandler = new OnExportHandler(loggerFactory.CreateLogger<OnExportHandler>(), exportTasks);
    }

    public Task StartAsync<TContext>(IHttpApplication<TContext> application, CancellationToken cancellationToken)
        where TContext : notnull
    {
        if (_isExporting)
        {
            _logger.LogDebug("Export server started.");

            //this method executes after all IHostedService's started,
            //so it's safe for consumer add some tasks that executing on app startup
            _exportTask = ExecuteAsync(_stoppingCts.Token);

            //if the task is completed then return it, this will bubble cancellation and failure to the caller
            if (_exportTask.IsCompleted)
            {
                return _exportTask;
            }

            return Task.CompletedTask;
        }

        return _originalServer.StartAsync(application, cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_isExporting)
        {
            //stop called without start
            if (_exportTask == null)
            {
                return;
            }

            try
            {
                //signal cancellation to the executing method
                _stoppingCts.Cancel();
            }
            finally
            {
                await Task.WhenAny(_exportTask, Task.Delay(Timeout.Infinite, cancellationToken));
            }

            _logger.LogDebug("Export server stopped.");
            return;
        }

        await _originalServer.StopAsync(cancellationToken);
    }

    public void Dispose()
    {
        _originalServer.Dispose();
    }

    private async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        await _onExportHandler.HandleExportAsync(cancellationToken);
        _hostApplicationLifetime.StopApplication();
    }

    private class OnExportHandler
    {
        private readonly ILogger<OnExportHandler> _logger;
        private readonly IExportTask[] _exportTasks;

        public OnExportHandler(ILogger<OnExportHandler> logger, IEnumerable<IExportTask> exportTasks)
        {
            _logger = logger;
            _exportTasks = exportTasks.ToArray();
        }

        public async Task HandleExportAsync(CancellationToken cancellationToken)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Executing {ExportTasksCount} export tasks ({ExportTasks}).",
                    _exportTasks.Length, string.Join(", ", _exportTasks.Select(x => x.Name)));
            }

            var stopwatch = Stopwatch.StartNew();

            foreach (var exportTask in _exportTasks)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    await ExecuteExportTask(exportTask, cancellationToken);
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Task '{ExportTask}' execution failed with an exception. Stopping application.",
                        exportTask.Name);
                    return;
                }
            }

            stopwatch.Stop();
            _logger.LogInformation("All export operations completed in {ElapsedMs}ms.", stopwatch.ElapsedMilliseconds);
        }

        private async Task ExecuteExportTask(IExportTask exportTask, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Task '{ExportTask}' started.", exportTask.Name);

            var stopwatch = Stopwatch.StartNew();

            await exportTask.ExecuteAsync(cancellationToken);

            stopwatch.Stop();
            _logger.LogInformation("Task '{ExportTask}' completed in {ElapsedMs}ms.", exportTask.Name,
                stopwatch.ElapsedMilliseconds);
        }
    }
}