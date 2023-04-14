using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Osnova.Shiki.Infrastructure;

namespace Osnova.Shiki.Cli;

public class ShikiCli
{
    private readonly ILoggerFactory _loggerFactory;
    private readonly CliDownloader _cliDownloader;

    private bool _initialized;
    private string? _binPath;

    public ShikiCli(ILoggerFactory loggerFactory, CliDownloader cliDownloader)
    {
        _loggerFactory = loggerFactory;
        _cliDownloader = cliDownloader;
    }

    public async Task InitializeAsync(string? version = null)
    {
        string useVersion = version ?? Upstream.Version;
        string? binName = Upstream.GetNativeExecutableName();

        if (binName == null)
        {
            throw new ShikiCliException(
                $"shiki-cli does not support the {RuntimeInformation.RuntimeIdentifier} platform");
        }

        string installLocation = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!;
        string storeFolderPath = Path.Combine(installLocation, "shiki-cli");

        _binPath = Path.GetFullPath(GetStoreBinName(binName, useVersion), storeFolderPath);

        if (!File.Exists(_binPath))
        {
            if (Directory.Exists(storeFolderPath))
            {
                // Remove all temp files if any
                foreach (string filePath in Directory.GetFiles(storeFolderPath, "*.tmp"))
                {
                    File.Delete(filePath);
                }
            }
            else
            {
                Directory.CreateDirectory(storeFolderPath);
            }

            string tempBinPath = Path.GetFullPath($"{Guid.NewGuid():N}.tmp", storeFolderPath);

            await _cliDownloader.DownloadAsync(useVersion, binName, tempBinPath);

            // If running on a Unix-based platform give file permission to be executed
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ||
                RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                await ProcessUtil.RunAsync("chmod", "+x " + tempBinPath);

                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    await ProcessUtil.RunAsync("xattr", "-d com.apple.quarantine " + tempBinPath);
                }
            }

            // Rename file
            File.Move(tempBinPath, _binPath);
        }

        _initialized = true;
    }

    private string GetStoreBinName(string binName, string version)
    {
        string fileName = Path.GetFileNameWithoutExtension(binName);
        string extension = Path.GetExtension(binName) ?? "";
        version = version.Replace('.', '_');

        return $"{fileName}-{version}{extension}";
    }

    public string Executable()
    {
        if (!_initialized)
        {
            throw new InvalidOperationException("Must be initialized before any other action.");
        }

        return _binPath!;
    }

    public CliExe HighlightCommand(string inputName, string outputName,
        string language, string? theme = null)
    {
        IEnumerable<string> args = new[]
        {
            "-i", inputName,
            "-o", outputName,
            "-l", language,
        };

        if (theme != null)
        {
            args = args.Append("-t");
            args = args.Append(theme);
        }

        return new CliExe(_loggerFactory, Executable(), string.Join(' ', args));
    }
}