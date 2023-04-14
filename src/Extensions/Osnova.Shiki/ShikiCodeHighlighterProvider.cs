using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Osnova.Markdown.CodeHighlight;
using Osnova.Shiki.Cli;

namespace Osnova.Shiki;

public class ShikiCodeHighlighterProvider : BackgroundService, ICodeHighlighterProvider
{
    private readonly ShikiHighlighterWrap _highlighter;

    public ShikiCodeHighlighterProvider(ILoggerFactory loggerFactory,
        IOptions<ShikiCodeHighlighterOptions> options, IHostEnvironment hostEnvironment)
    {
        var optionsValue = options.Value;

        string tempFolder = optionsValue.TempFolderPath == null
            ? Path.Combine(hostEnvironment.ContentRootPath, "Temp", "Shiki")
            : Path.GetFullPath(optionsValue.TempFolderPath);

        _highlighter = new ShikiHighlighterWrap(loggerFactory, tempFolder, optionsValue.HighlightTheme);
    }

    public ICodeHighlighter GetCodeHighlighter()
    {
        return _highlighter;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            // Bootstrap deps in background
            await _highlighter.InitializeShikiCli();
        }
        catch
        {
            // Ignore this error from here nd don't rethrow
        }
    }

    private class ShikiHighlighterWrap : ICodeHighlighter
    {
        private readonly ILogger<ShikiHighlighterWrap> _logger;

        private readonly ShikiCodeHighlighter _highlighter;
        private readonly ShikiCli _shikiCli;
        private readonly string _tempFolder;

        private readonly SemaphoreSlim _initializeLock = new(1);
        private bool _initialized;

        public ShikiHighlighterWrap(ILoggerFactory loggerFactory, string tempFolder, string? highlightTheme)
        {
            _logger = loggerFactory.CreateLogger<ShikiHighlighterWrap>();
            _tempFolder = tempFolder;

            var downloader = new CliDownloader(loggerFactory.CreateLogger<CliDownloader>());
            _shikiCli = new ShikiCli(loggerFactory, downloader);
            _highlighter = new ShikiCodeHighlighter(_shikiCli, tempFolder, highlightTheme);
        }

        public async Task<string> HighlightAsync(string code, string languageCode)
        {
            if (!_initialized)
            {
                await InitializeShikiCli();
            }

            return await _highlighter.HighlightAsync(code, languageCode);
        }

        public async Task InitializeShikiCli()
        {
            if (!_initialized)
            {
                await _initializeLock.WaitAsync();
                try
                {
                    if (!_initialized)
                    {
                        // Create or clean temp directory
                        var temp = new DirectoryInfo(_tempFolder);
                        if (!temp.Exists)
                        {
                            temp.Create();
                        }
                        else
                        {
                            CleanDirectory(temp);
                        }

                        // Do actual initialization that is required for cli to work
                        await _shikiCli.InitializeAsync(Upstream.Version);
                        _initialized = true;
                    }
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "Failed to initialize shiki-cli");
                    throw;
                }
                finally
                {
                    _initializeLock.Release();
                }
            }
        }

        private void CleanDirectory(DirectoryInfo dir)
        {
            foreach (FileInfo file in dir.EnumerateFiles())
            {
                try
                {
                    file.Delete();
                }
                catch
                {
                    // ignored
                }
            }

            foreach (DirectoryInfo subdir in dir.EnumerateDirectories())
            {
                try
                {
                    subdir.Delete(true);
                }
                catch
                {
                    // ignored
                }
            }
        }
    }
}