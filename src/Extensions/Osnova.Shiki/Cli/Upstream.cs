using System.Runtime.InteropServices;

namespace Osnova.Shiki.Cli;

public class Upstream
{
    public static string Version => "v1.0.0-beta.1";

    public static string? GetNativeExecutableName()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "shikicli-win-arm64.exe",
                Architecture.X64 => "shikicli-win-x64.exe",
                _ => null
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "shikicli-macos-arm64",
                Architecture.X64 => "shikicli-macos-x64",
                _ => null
            };
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return RuntimeInformation.ProcessArchitecture switch
            {
                Architecture.Arm64 => "shikicli-linux-arm64",
                Architecture.Arm => "shikicli-linux-armv7",
                Architecture.X64 => "shikicli-linux-x64",
                _ => null
            };
        }

        return null;
    }
}