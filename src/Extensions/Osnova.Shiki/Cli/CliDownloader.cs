using System.Net;
using Microsoft.Extensions.Logging;

namespace Osnova.Shiki.Cli;

public class CliDownloader
{
    private readonly ILogger<CliDownloader> _logger;

    public CliDownloader(ILogger<CliDownloader> logger)
    {
        _logger = logger;
    }

    public async Task DownloadAsync(string version, string binName, string saveBinPath)
    {
        string url = $"https://github.com/rozumak/shiki-cli/releases/download/{version}/{binName}";

        _logger.LogInformation("Downloading shiki cli {Version} from {Url}.", version, url);

        try
        {
            await DownloadFileAsync(url, saveBinPath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to download and save native shiki-cli.");
            throw;
        }
    }

    private async Task DownloadFileAsync(string url, string saveBinPath)
    {
        using HttpClient client = new HttpClient();
        using HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            throw new ShikiCliException($"Cannot find the shiki-cli executable via url {url}.");
        }

        response.EnsureSuccessStatusCode();


        await using var downloadStream = await client.GetStreamAsync(url);

        try
        {
            await using var output = File.Open(saveBinPath, FileMode.Create);
            await downloadStream.CopyToAsync(output);

            await output.FlushAsync();
        }
        catch
        {
            // Silently try remove created temp file when failed to download
            try
            {
                File.Delete(saveBinPath);
            }
            catch
            {
                // ignored
            }

            throw;
        }
    }
}