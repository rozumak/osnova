using Markdig;

namespace Osnova.Markdown.CodeHighlight;

public static class MarkdownHighlightExtensions
{
    public static MarkdownPipelineBuilder UseHighlightCode(this MarkdownPipelineBuilder pipeline)
    {
        pipeline.Extensions.Add(new HighlightCodeExtension());
        return pipeline;
    }
}