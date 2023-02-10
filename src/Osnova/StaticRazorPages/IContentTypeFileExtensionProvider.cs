namespace Osnova.StaticRazorPages;

public interface IContentTypeFileExtensionProvider
{
    bool TryGetFileExtension(string contentType, out string? extension);
}