using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Osnova.FileStorages;

namespace Osnova.ExportTasks;

public class ExportStaticWebFiles : IExportTask
{
    private readonly ILogger<ExportStaticWebFiles> _logger;

    private readonly IWebHostEnvironment _hostEnvironment;
    private readonly IFileStorage _fileStorage;

    public string Name => "Export static web files";

    public ExportStaticWebFiles(ILogger<ExportStaticWebFiles> logger, 
        IWebHostEnvironment hostEnvironment, IFileStorage fileStorage)
    {
        _logger = logger;
        _hostEnvironment = hostEnvironment;
        _fileStorage = fileStorage;
    }

    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var fileProvider = _hostEnvironment.WebRootFileProvider;
        
        var outputDir = _fileStorage.GetDirectory("");
        await CopyFilesInDirectory(fileProvider, outputDir.Subpath, outputDir, cancellationToken);
    }

    private async Task CopyFilesInDirectory(IFileProvider fileProvider,
        string copyFromPath, IFileStorageDirectory outputDir, CancellationToken cancellationToken)
    {
        var files = fileProvider.GetDirectoryContents(copyFromPath);

        foreach (var file in files)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.IsDirectory)
            {
                var subdir = outputDir.GetSubDirectory(file.Name);
                await CopyFilesInDirectory(fileProvider, subdir.Subpath, subdir, cancellationToken);
            }
            else
            {
                await using var destFileStream = outputDir.CreateWriteStream(file.Name);
                await using var fileStream = file.CreateReadStream();

                await fileStream.CopyToAsync(destFileStream, cancellationToken);
            }
        }
    }
}