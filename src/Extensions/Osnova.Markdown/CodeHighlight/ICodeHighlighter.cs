namespace Osnova.Markdown.CodeHighlight;

public interface ICodeHighlighter
{
    string Highlight(string code, string languageCode);
}