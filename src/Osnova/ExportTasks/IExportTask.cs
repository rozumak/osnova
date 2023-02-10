namespace Osnova.ExportTasks;

public interface IExportTask
{
    string Name { get; }

    Task ExecuteAsync(CancellationToken cancellationToken);
}