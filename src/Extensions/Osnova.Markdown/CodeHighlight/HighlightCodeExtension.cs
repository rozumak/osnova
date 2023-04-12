using Markdig;
using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Osnova.Markdown.CodeHighlight;

public class HighlightCodeExtension : IMarkdownExtension
{
    public void Setup(MarkdownPipelineBuilder pipeline)
    {
        pipeline.BlockParsers.Replace<FencedCodeBlockParser>(new HighlightFencedCodeBlockParser());
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        var codeBlockRenderer = renderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
        if (codeBlockRenderer != null)
        {
            var newCodeBlockRenderer = new HighlightCodeBlockRenderer(codeBlockRenderer);
            renderer.ObjectRenderers.Replace<CodeBlockRenderer>(newCodeBlockRenderer);
        }
    }
}