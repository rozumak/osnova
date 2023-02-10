namespace Osnova.Markdown.CodeHighlight;

public interface ICodeHighlighterProvider
{
    ICodeHighlighter GetCodeHighlighter();
}