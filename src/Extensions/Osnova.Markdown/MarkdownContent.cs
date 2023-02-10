using Markdig;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Osnova.Markdown;

public class MarkdownContent
{
    private readonly MarkdownPipeline _pipeline;

    private Func<string, string>? _linkRewriter;

    public MarkdownDocument Document { get; }

    public string? FileLocation { get; }

    public Action<TextWriter> Render => RenderCore;

    public MarkdownContent(MarkdownPipeline pipeline, MarkdownDocument document, string? fileLocation = null)
    {
        _pipeline = pipeline;
        Document = document;
        FileLocation = fileLocation;
    }

    private void RenderCore(TextWriter writer)
    {
        var renderer = new HtmlRenderer(writer)
        {
            LinkRewriter = _linkRewriter
        };
        _pipeline.Setup(renderer);

        renderer.Render(Document);
    }

    public void SetupLinkRewriter(Func<string, string> linkRewriter)
    {
        _linkRewriter = linkRewriter;
    }
}