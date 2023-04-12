using Markdig.Renderers;
using Markdig;
using Markdig.Syntax;

namespace Osnova.Markdown;

public class MarkdownResult<T>
{
    private readonly MarkdownPipeline _pipeline;
    private readonly List<Func<Task>>? _asyncRenderTasks;

    public string FileName { get; set; }

    public MarkdownDocument Document { get; }

    public T? Model { get; }

    public MarkdownResult(MarkdownPipeline pipeline, List<Func<Task>>? asyncRenderTasks, 
        MarkdownDocument document, string fileName, T? frontMatterModel)
    {
        _pipeline = pipeline;
        _asyncRenderTasks = asyncRenderTasks;
        Document = document;
        FileName = fileName;
        Model = frontMatterModel;
    }

    public async Task<MarkdownContent> RenderAsync(Func<string, string>? linkRewriter = null)
    {
        var memoryStream = new MemoryStream();
        await using StreamWriter writer = new StreamWriter(memoryStream, leaveOpen: true);

        var renderer = new HtmlRenderer(writer)
        {
            LinkRewriter = linkRewriter
        };
        _pipeline.Setup(renderer);

        if (_asyncRenderTasks != null)
        {
            foreach (var asyncRenderTask in _asyncRenderTasks)
            {
                await asyncRenderTask.Invoke();
            }
        }

        renderer.Render(Document);

        return new MarkdownContent(memoryStream);
    }
}