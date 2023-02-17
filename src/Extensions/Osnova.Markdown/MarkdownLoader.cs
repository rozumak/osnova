using Markdig;
using Microsoft.Extensions.Options;

namespace Osnova.Markdown;

public class MarkdownLoader : IMarkdownLoader
{
    private readonly MarkdownLoaderOptions _options;
    private readonly MarkdownPipeline _pipeline;

    public MarkdownLoader(IOptions<MarkdownLoaderOptions> options)
    {
        _options = options.Value;
        _pipeline = _options.PipelineFactory();
    }

    public async Task<MarkdownResult<T>> LoadMarkdownPageAsync<T>(string fileName)
    {
        string markdown = await File.ReadAllTextAsync(fileName);

        var document = Markdig.Markdown.Parse(markdown, _pipeline);

        // Try extract the front matter from markdown document
        T? frontMatterModel = default;
        foreach (var frontMatterExtractor in _options.FrontMatterExtractors)
        {
            if (frontMatterExtractor.Extract(document, out frontMatterModel))
            {
                break;
            }
        }

        //TODO: make file name relative
        var content = new MarkdownContent(_pipeline, document, fileName);
        return new MarkdownResult<T>(content, frontMatterModel);
    }
}