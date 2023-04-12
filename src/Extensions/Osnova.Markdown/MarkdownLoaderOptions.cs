using Markdig;
using Osnova.Markdown.CodeHighlight;
using Osnova.Markdown.FrontMatterExtractors;

namespace Osnova.Markdown;

public class MarkdownLoaderOptions
{
    public IList<IFrontMatterExtractor> FrontMatterExtractors { get; }

    public ICodeHighlighterProvider? CodeHighlighterProvider { get; private set; }

    public Type? CodeHighlighterProviderType { get; private set; }

    public Func<MarkdownPipeline> PipelineFactory { get; set; }

    public MarkdownLoaderOptions()
    {
        FrontMatterExtractors = new List<IFrontMatterExtractor>
        {
            new YamlFrontMatterExtractor()
        };

        PipelineFactory = CreatePipeline;
    }

    public MarkdownLoaderOptions UseCodeHighlighterProvider(ICodeHighlighterProvider codeHighlighterProvider)
    {
        CodeHighlighterProvider = codeHighlighterProvider;
        CodeHighlighterProviderType = null;

        return this;
    }

    public MarkdownLoaderOptions UseCodeHighlighterProvider<T>()
        where T : ICodeHighlighterProvider
    {
        CodeHighlighterProvider = null;
        CodeHighlighterProviderType = typeof(T);

        return this;
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

        if (CodeHighlighterProvider != null || CodeHighlighterProviderType != null)
        {
            builder.UseHighlightCode();
        }

        return builder.Build();
    }
}