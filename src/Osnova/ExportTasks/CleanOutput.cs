using Microsoft.Extensions.Logging;
using Osnova.FileStorages;

namespace Osnova.ExportTasks;

public class CleanOutput : IExportTask
{
    private readonly ILogger<CleanOutput> _logger;

    private readonly IFileStorage _fileStorage;

    public string Name => "Clean output";

    public CleanOutput(ILogger<CleanOutput> logger, IFileStorage fileStorage)
    {
        _logger = logger;
        _fileStorage = fileStorage;
    }

    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _fileStorage.GetDirectory("").Clean();
        return Task.CompletedTask;
    }
}