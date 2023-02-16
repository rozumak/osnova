using Markdig;
using Markdig.Syntax;

namespace Osnova.Markdown.FrontMatterExtractors;

public interface IFrontMatterExtractor
{
    void Setup(MarkdownPipelineBuilder pipeline);

    bool Extract<T>(MarkdownDocument document, out T? result);
}