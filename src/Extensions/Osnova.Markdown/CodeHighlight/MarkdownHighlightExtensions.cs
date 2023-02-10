using Markdig;

namespace Osnova.Markdown.CodeHighlight;

public static class MarkdownHighlightExtensions
{
    public static MarkdownPipelineBuilder UseHighlightCode(this MarkdownPipelineBuilder pipeline,
        ICodeHighlighterProvider codeHighlighterProvider)
    {
        pipeline.Extensions.Add(new HighlightCodeExtension(codeHighlighterProvider));
        return pipeline;
    }
}