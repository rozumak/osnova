using Osnova.Markdown.CodeHighlight;
using Osnova.Shiki.Cli;

namespace Osnova.Shiki;

internal class ShikiCodeHighlighter : ICodeHighlighter
{
    private readonly string _tempFolder;
    private readonly string? _highlightTheme;

    private ShikiCli _shikiCli;

    public ShikiCodeHighlighter(ShikiCli shikiCli, string tempFolder, string? highlightTheme)
    {
        _tempFolder = tempFolder;
        _highlightTheme = highlightTheme;
        _shikiCli = shikiCli;
    }

    public async Task<string> HighlightAsync(string code, string languageCode)
    {
        string name = $"{Guid.NewGuid():N}";
        string inputFileName = Path.GetFullPath($"{name}.tmp", _tempFolder);
        string outputFileName = Path.GetFullPath($"{name}.out.tmp", _tempFolder);
        await File.WriteAllTextAsync(inputFileName, code);

        CliExe command = _shikiCli.HighlightCommand(inputFileName, outputFileName, languageCode, _highlightTheme);
        command.WorkingDirectory = _tempFolder;
        int exitCode = await command.RunAsync();

        if (exitCode != 0)
        {
            throw new ShikiException($"Failed to highlight code. Shiki-cli exit code: {exitCode}.");
        }

        string coloredCode = await File.ReadAllTextAsync(outputFileName);
        return coloredCode;
    }
}