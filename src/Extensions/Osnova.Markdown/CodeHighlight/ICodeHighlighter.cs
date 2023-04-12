namespace Osnova.Markdown.CodeHighlight;

public interface ICodeHighlighter
{
    Task<string> HighlightAsync(string code, string languageCode);
}