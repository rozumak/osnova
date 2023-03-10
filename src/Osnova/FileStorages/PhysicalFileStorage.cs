using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Osnova.FileStorages;

public class PhysicalFileStorage : IFileStorage
{
    private readonly string _root;

    public PhysicalFileStorage(IOptions<StaticExportOptions> options,
        IWebHostEnvironment hostEnvironment)
    {
        string? outputDirectory = options.Value.OutputDirectory;
        if (outputDirectory == null)
        {
            _root = Path.Combine(hostEnvironment.ContentRootPath, "output");
        }
        else
        {
            _root = Path.IsPathRooted(outputDirectory)
                ? outputDirectory
                : Path.GetFullPath(outputDirectory);
        }
    }

    public IFileStorageDirectory GetDirectory(string subpath)
    {
        return new FileStorageDirectory(subpath, _root).CreateIfNotExist();
    }

    private class FileStorageDirectory : IFileStorageDirectory
    {
        private readonly string _fullPath;
        private readonly string _root;

        public string Subpath { get; }

        public FileStorageDirectory(string subpath, string basePath)
        {
            _root = basePath;
            _fullPath = Path.GetFullPath(subpath, basePath);
            Subpath = subpath;
        }

        public Stream CreateWriteStream(string fileName)
        {
            string fullFilePath = Path.GetFullPath(fileName, _fullPath);
            return new FileStream(fullFilePath, FileMode.Create);
        }

        public IFileStorageDirectory GetSubDirectory(string subpath)
        {
            subpath = Path.Combine(Subpath, subpath);
            return new FileStorageDirectory(subpath, _root).CreateIfNotExist();
        }

        public void Clean()
        {
            DirectoryInfo di = new DirectoryInfo(_root);

            foreach (FileInfo file in di.EnumerateFiles())
            {
                file.Delete();
            }

            foreach (DirectoryInfo dir in di.EnumerateDirectories())
            {
                dir.Delete(true);
            }
        }

        public FileStorageDirectory CreateIfNotExist()
        {
            Directory.CreateDirectory(_fullPath);
            return this;
        }
    }
}