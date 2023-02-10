using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Osnova;

public class HtmlRawContent : IHtmlContent
{
    private readonly Action<TextWriter, HtmlEncoder> _render;

    public HtmlRawContent(Action<TextWriter> render)
    {
        _render = (writer, encoder) => render(writer);
    }

    public HtmlRawContent(Action<TextWriter, HtmlEncoder> render)
    {
        _render = render;
    }

    public void WriteTo(TextWriter writer, HtmlEncoder encoder)
    {
        _render(writer, encoder);
    }
}

public static class HtmlRawContentExtensions
{
    public static HtmlRawContent ToHtmlContent(this Action<TextWriter, HtmlEncoder> render)
    {
        return new HtmlRawContent(render);
    }

    public static HtmlRawContent ToHtmlContent(this Action<TextWriter> render)
    {
        return new HtmlRawContent(render);
    }
}