namespace Osnova.Markdown;

public class MarkdownContent: IDisposable
{
    private readonly Stream _buffer;

    public MarkdownContent(Stream buffer)
    {
        _buffer = buffer;
    }

    public void WriteTo(TextWriter writer)
    {
        // Move the stream pointer to the beginning
        _buffer.Seek(0, SeekOrigin.Begin);

        using var reader = new StreamReader(_buffer, leaveOpen: true);
        var buffer = new char[4096];
        
        int bytesRead;
        while ((bytesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
        {
            writer.Write(buffer, 0, bytesRead);
        }
    }

    public void Dispose()
    {
        _buffer.Dispose();
    }
}