using Microsoft.Extensions.Logging;
using Osnova.Shiki.Infrastructure;

namespace Osnova.Shiki.Cli
{
    public class CliExe
    {
        private readonly ILoggerFactory _loggerFactory;

        public string FileName { get; }

        public string? Arguments { get; }

        public string? WorkingDirectory { get; set; }

        public CliExe(ILoggerFactory loggerFactory, string fileName, 
            string? arguments, string? workingDirectory = null)
        {
            _loggerFactory = loggerFactory;
            FileName = fileName;
            Arguments = arguments;
            WorkingDirectory = workingDirectory;
        }

        public async Task<int> RunAsync()
        {
            var logger = _loggerFactory.CreateLogger<CliExe>();
            var sw = new StringWriter();
            void WriteOutMessage(string message)
            {
                if (!message.StartsWith("Supported languages"))
                    sw.WriteLine(message);
            }

            // Execute external process
            var result = await ProcessUtil.RunAsync(FileName, Arguments ?? "",
                workingDirectory: WorkingDirectory,
                outputDataReceived: WriteOutMessage, errorDataReceived: WriteOutMessage,
                throwOnError: false);

            await sw.FlushAsync();
            if (sw.GetStringBuilder().Length > 0)
            {
                if (result.ExitCode == 0)
                {
                    logger.LogInformation("Shiki-cli: {Message}", sw);
                }
                else
                {
                    logger.LogWarning("Shiki-cli: {Message}", sw);
                }
            }

            return result.ExitCode;
        }

        public override string ToString()
        {
            return $"Execute command: {FileName} {Arguments}";
        }
    }
}
