namespace Osnova.FileStorages;

public interface IFileStorage
{
    IFileStorageDirectory GetDirectory(string subpath);
}