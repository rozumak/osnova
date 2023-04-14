using Microsoft.Extensions.DependencyInjection.Extensions;
using Osnova.Markdown.CodeHighlight;
using Osnova.Shiki;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddShikiCodeHighlighter(this IServiceCollection services,
        Action<ShikiCodeHighlighterOptions>? configure = null)
    {
        services.Configure<ShikiCodeHighlighterOptions>(
            options => { configure?.Invoke(options); });

        services.TryAddSingleton<ShikiCodeHighlighterProvider>();
        services.AddTransient<ICodeHighlighterProvider>(sp =>
            sp.GetRequiredService<ShikiCodeHighlighterProvider>());
        services.AddHostedService(sp =>
            sp.GetRequiredService<ShikiCodeHighlighterProvider>());

        return services;
    }
}