namespace Osnova.Markdown;

public interface IMarkdownLoader
{
    Task<MarkdownResult<T>> LoadMarkdownPageAsync<T>(string fileName);
}