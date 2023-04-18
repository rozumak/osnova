using Markdig;
using Markdig.Syntax;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osnova.Markdown.CodeHighlight;
using Osnova.Markdown.FrontMatterExtractors;

namespace Osnova.Markdown;

public class MarkdownLoader : IMarkdownLoader
{
    private readonly ILogger<MarkdownLoader> _logger;

    private readonly IList<IFrontMatterExtractor> _frontMatterExtractors;
    private readonly ICodeHighlighterProvider? _codeHighlighterProvider;
    private readonly MarkdownPipeline _pipeline;

    public MarkdownLoader(ILogger<MarkdownLoader> logger, IOptions<MarkdownLoaderOptions> options,
        IServiceProvider serviceProvider)
    {
        _logger = logger;

        var optionsVal = options.Value;
        _pipeline = optionsVal.PipelineFactory();

        _frontMatterExtractors = optionsVal.FrontMatterExtractors;

        // Take from options if specified, otherwise try get from services
        _codeHighlighterProvider = optionsVal.CodeHighlighterProvider ??
                                   serviceProvider.GetService<ICodeHighlighterProvider>();
    }

    public async Task<MarkdownResult<T>> LoadMarkdownPageAsync<T>(string fileName)
    {
        string markdown = await File.ReadAllTextAsync(fileName);

        var document = Markdig.Markdown.Parse(markdown, _pipeline);

        // Try extract the front matter from markdown document
        T? frontMatterModel = default;
        foreach (var frontMatterExtractor in _frontMatterExtractors)
        {
            if (frontMatterExtractor.Extract(document, out frontMatterModel))
            {
                break;
            }
        }

        List<Func<Task>>? asyncRenderTasks = null;

        // Create highlight render task for this document if needed, that will be called on markdown render
        if (_codeHighlighterProvider != null)
        {
            var codeBlocks = document
                .Descendants<HighlightFencedCodeBlock>()
                .ToArray();
            if (codeBlocks.Length > 0)
            {
                asyncRenderTasks ??= new List<Func<Task>>(1);
                asyncRenderTasks.Add(async () =>
                {
                    try
                    {
                        await HighlightCodeAsync(_codeHighlighterProvider, codeBlocks, fileName);
                    }
                    catch (Exception e)
                    {
                        _logger.LogError(e, "Failed to highlight code blocks in '{FileName}'.", fileName);
                    }
                });
            }
        }

        //TODO: make file name relative
        return new MarkdownResult<T>(_pipeline, asyncRenderTasks, document, fileName, frontMatterModel);
    }

    private async Task HighlightCodeAsync(ICodeHighlighterProvider highlightProvider,
        HighlightFencedCodeBlock[] codeBlocks, string fileName)
    {
        var codeHighlighter = highlightProvider.GetCodeHighlighter();

        foreach (var codeBlock in codeBlocks)
        {
            string? lang = codeBlock.ExtractLanguage();
            if (lang != null)
            {
                try
                {
                    string code = codeBlock.ExtractCode();
                    string coloredCode = await codeHighlighter.HighlightAsync(code, lang);
                    codeBlock.HighlightedCode = coloredCode;
                }
                catch (Exception e)
                {
                    _logger.LogWarning(e,
                        "Failed to highlight code block with language '{Language}' in '{FileName}'.",
                        lang, fileName);
                }
            }
        }
    }
}