using Osnova.Markdown;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddOsnovaMarkdown(this IServiceCollection services,
        Action<MarkdownLoaderOptions>? configure = null)
    {
        services.Configure<MarkdownLoaderOptions>(c => configure?.Invoke(c));
        services.AddSingleton<IMarkdownLoader, MarkdownLoader>();
        return services;
    }
}
