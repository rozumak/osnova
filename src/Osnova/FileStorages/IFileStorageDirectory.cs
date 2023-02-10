namespace Osnova.FileStorages;

public interface IFileStorageDirectory
{
    string Subpath { get; }

    Stream CreateWriteStream(string fileName);

    IFileStorageDirectory GetSubDirectory(string subpath);

    void Clean();
}