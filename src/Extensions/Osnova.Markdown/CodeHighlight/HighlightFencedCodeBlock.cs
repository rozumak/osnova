using Markdig.Parsers;
using Markdig.Renderers;
using Markdig.Syntax;

namespace Osnova.Markdown.CodeHighlight;

public class HighlightFencedCodeBlock : FencedCodeBlock
{
    public string? HighlightedCode { get; set; }

    public HighlightFencedCodeBlock(BlockParser parser) : base(parser)
    {
    }

    public string? ExtractLanguage()
    {
        string? fencedCodeBlockInfo = Info;
        if (fencedCodeBlockInfo != null)
        {
            //default value "language-"
            string infoPrefix = (Parser as FencedCodeBlockParser)?.InfoPrefix ??
                                FencedCodeBlockParser.DefaultInfoPrefix;

            //get language code without prefix
            string languageCode = fencedCodeBlockInfo;
            if (languageCode.StartsWith(infoPrefix, StringComparison.OrdinalIgnoreCase))
            {
                languageCode = fencedCodeBlockInfo.Remove(0, infoPrefix.Length);
            }

            return languageCode;
        }

        return null;
    }

    public string ExtractCode()
    {
        StringWriter stringWriter = new StringWriter();
        HtmlRenderer internalRenderer = new HtmlRenderer(stringWriter);
        internalRenderer.WriteLeafRawLines(this, false, false);
        string code = stringWriter.GetStringBuilder().ToString();

        return code;
    }
}