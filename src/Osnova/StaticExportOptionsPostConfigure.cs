using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Options;

namespace Osnova;

internal class StaticExportOptionsPostConfigure : IPostConfigureOptions<StaticExportOptions>
{
    private readonly IConfiguration _configuration;

    public StaticExportOptionsPostConfigure(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void PostConfigure(string name, StaticExportOptions options)
    {
        if (options.RunExportCommand)
            return;

        ConfigureFromCommandLine(_configuration, options);
    }

    private void ConfigureFromCommandLine(IConfiguration configuration, StaticExportOptions options)
    {
        var arguments = Environment.GetCommandLineArgs();

        if (arguments.Length > 1)
        {
            string command = arguments[1];
            if (command.Equals("export", StringComparison.OrdinalIgnoreCase))
            {
                options.RunExportCommand = true;

                if (options.OutputDirectory == null)
                {
                    var switchMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                    {
                        {"-o", "output"},
                    };

                    var cmdProvider = new CommandLineConfigurationProvider(arguments.Skip(2), switchMappings);
                    cmdProvider.Load();
                    cmdProvider.TryGet("output", out string outputValue);
                    options.OutputDirectory = outputValue;
                }
            }
        }
    }
}