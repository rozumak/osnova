namespace Osnova.Markdown;

public class MarkdownWithFrontMatter<T>
{
    public MarkdownContent MarkdownContent { get; }

    public T? FrontMatterModel { get; }

    public MarkdownWithFrontMatter(MarkdownContent content, T? frontMatterModel)
    {
        FrontMatterModel = frontMatterModel;
        MarkdownContent = content;
    }
}