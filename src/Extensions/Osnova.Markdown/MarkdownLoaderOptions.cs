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
        if (CodeHighlighterProvider != null)
        {
            builder.UseHighlightCode(CodeHighlighterProvider);
        }

        foreach (var frontMatterExtractor in FrontMatterExtractors)
        {
            frontMatterExtractor.Setup(builder);
        }

        return builder.Build();
    }
}