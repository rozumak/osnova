using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Osnova.Markdown.CodeHighlight;

public class HighlightCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly CodeBlockRenderer _codeBlockRenderer;

    public HighlightCodeBlockRenderer(CodeBlockRenderer? codeBlockRenderer)
    {
        _codeBlockRenderer = codeBlockRenderer ?? new CodeBlockRenderer();
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        if (obj is HighlightFencedCodeBlock {HighlightedCode: { }} highlightCodeBlock)
        {
            renderer.Write(highlightCodeBlock.HighlightedCode);
        }
        else
        {
            _codeBlockRenderer.Write(renderer, obj);
        }
    }
}