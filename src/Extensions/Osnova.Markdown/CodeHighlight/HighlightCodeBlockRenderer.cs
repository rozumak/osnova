using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;

namespace Osnova.Markdown.CodeHighlight;

public class HighlightCodeBlockRenderer : HtmlObjectRenderer<CodeBlock>
{
    private readonly ICodeHighlighterProvider _codeHighlighterProvider;
    private readonly CodeBlockRenderer _codeBlockRenderer;

    public HighlightCodeBlockRenderer(ICodeHighlighterProvider codeHighlighterProvider,
        CodeBlockRenderer? codeBlockRenderer)
    {
        _codeHighlighterProvider = codeHighlighterProvider;
        _codeBlockRenderer = codeBlockRenderer ?? new CodeBlockRenderer();
    }

    protected override void Write(HtmlRenderer renderer, CodeBlock obj)
    {
        string? languageCode = null;
        string? fencedCodeBlockInfo = (obj as FencedCodeBlock)?.Info;
        if (fencedCodeBlockInfo != null)
        {
            //default value "language-"
            string infoPrefix = (obj.Parser as FencedCodeBlockParser)?.InfoPrefix ??
                                FencedCodeBlockParser.DefaultInfoPrefix;

            //get language code without prefix
            languageCode = fencedCodeBlockInfo;
            if (languageCode.StartsWith(infoPrefix, StringComparison.OrdinalIgnoreCase))
            {
                languageCode = fencedCodeBlockInfo.Remove(0, infoPrefix.Length);
            }
        }

        if (!string.IsNullOrWhiteSpace(languageCode))
        {
            WriteHighlighted(renderer, obj, languageCode);
        }
        else
        {
            _codeBlockRenderer.Write(renderer, obj);
        }
    }

    private void WriteHighlighted(HtmlRenderer renderer, CodeBlock obj, string languageCode)
    {
        //extract source code from code block
        StringWriter stringWriter = new StringWriter();
        HtmlRenderer internalRenderer = new HtmlRenderer(stringWriter);
        internalRenderer.WriteLeafRawLines(obj, false, false);
        string code = stringWriter.GetStringBuilder().ToString();

        var highlighter = _codeHighlighterProvider.GetCodeHighlighter();
        code = highlighter.Highlight(code, languageCode);

        //write result
        var attributes = new HtmlAttributes();
        attributes.AddClass($"language-{languageCode}");

        renderer.Write("<pre><code")
            .WriteAttributes(attributes)
            .Write(">")
            .Write(code)
            .WriteLine("</code></pre>");
    }
}