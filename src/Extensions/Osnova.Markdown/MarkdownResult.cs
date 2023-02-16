namespace Osnova.Markdown;

public class MarkdownResult<T>
{
    public MarkdownContent Content { get; }

    public T? Model { get; }

    public MarkdownResult(MarkdownContent content, T? model)
    {
        Model = model;
        Content = content;
    }
}