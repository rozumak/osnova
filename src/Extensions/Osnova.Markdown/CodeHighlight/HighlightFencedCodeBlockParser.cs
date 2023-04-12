using Markdig.Parsers;
using Markdig.Syntax;

namespace Osnova.Markdown.CodeHighlight;

public class HighlightFencedCodeBlockParser : FencedCodeBlockParser
{
    protected override FencedCodeBlock CreateFencedBlock(BlockProcessor processor)
    {
        var codeBlock = new HighlightFencedCodeBlock(this)
        {
            IndentCount = processor.Indent,
        };

        if (processor.TrackTrivia)
        {
            var linesBefore = processor.LinesBefore;
            processor.LinesBefore = null;

            codeBlock.LinesBefore = linesBefore;
            codeBlock.TriviaBefore = processor.UseTrivia(processor.Start - 1);
            codeBlock.NewLine = processor.Line.NewLine;
        }

        return codeBlock;
    }
}