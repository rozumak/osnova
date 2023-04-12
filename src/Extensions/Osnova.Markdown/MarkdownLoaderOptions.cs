using Markdig;
using Osnova.Markdown.CodeHighlight;
using Osnova.Markdown.FrontMatterExtractors;

namespace Osnova.Markdown;

public class MarkdownLoaderOptions
{
    public IList<IFrontMatterExtractor> FrontMatterExtractors { get; }

    public ICodeHighlighterProvider? CodeHighlighterProvider { get; set; }

    public Func<MarkdownPipeline> PipelineFactory { get; set; }

    public MarkdownLoaderOptions()
    {
        FrontMatterExtractors = new List<IFrontMatterExtractor>
        {
            new YamlFrontMatterExtractor()
        };

        PipelineFactory = CreatePipeline;
    }

    private MarkdownPipeline CreatePipeline()
    {
        var builder = new MarkdownPipelineBuilder();
        builder.Configure("common");

        // It's important that this setup go before code highlighter setup
        foreach (var frontMatterExtractor in FrontMatterExtractors)
        {
            frontMatterExtractor.Setup(builder);
        }

        if (CodeHighlighterProvider != null)
        {
            builder.UseHighlightCode();
        }

        return builder.Build();
    }
}