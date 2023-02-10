using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace Osnova.Markdown.CodeHighlight;

public class HighlightCodeExtension : IMarkdownExtension
{
    private readonly ICodeHighlighterProvider _codeHighlighterProvider;

    public HighlightCodeExtension(ICodeHighlighterProvider codeHighlighterProvider)
    {
        _codeHighlighterProvider = codeHighlighterProvider;
    }

    public void Setup(MarkdownPipelineBuilder pipeline)
    {
    }

    public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
    {
        var codeBlockRenderer = renderer.ObjectRenderers.FindExact<CodeBlockRenderer>();
        if (codeBlockRenderer != null)
        {
            var newCodeBlockRenderer = new HighlightCodeBlockRenderer(_codeHighlighterProvider, codeBlockRenderer);
            renderer.ObjectRenderers.Replace<CodeBlockRenderer>(newCodeBlockRenderer);
        }
    }
}