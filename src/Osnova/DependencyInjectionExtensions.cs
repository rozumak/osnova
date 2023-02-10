using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Osnova;
using Osnova.ExportTasks;
using Osnova.FileStorages;
using Osnova.StaticRazorPages;
using Osnova.StaticRazorPages.ApplicationModels;
using Osnova.StaticRazorPages.Infrastructure;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddOsnova(this IServiceCollection services,
        Action<StaticExportOptions>? configure = null)
    {
        services.Decorate<IServer, ExportServer>();
        services.Configure<RouteOptions>(options => options.LowercaseUrls = true);

        configure ??= _ => { };
        services.Configure<StaticExportOptions>(configure);
        services.AddSingleton<IPostConfigureOptions<StaticExportOptions>, StaticExportOptionsPostConfigure>();

        services.PostConfigure<ConsoleLifetimeOptions>(options => options.SuppressStatusMessages = true);

        //common services
        services.TryAddSingleton<IFileStorage, PhysicalFileStorage>();

        //NOTE: order is important, first add export razor pages task and then export static files task
        services.AddTransient<IExportTask, CleanOutput>();
        AddStaticPagesExportTask(services);
        services.AddTransient<IExportTask, ExportStaticWebFiles>();

        return services;
    }

    private static void AddStaticPagesExportTask(IServiceCollection services)
    {
        services.TryAddSingleton<IContentTypeFileExtensionProvider, ContentTypeFileExtensionProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IPostConfigureOptions<MvcOptions>, PostConfigureMvcOptions>());

        services.AddSingleton<StaticPageDescriptorRegistry>();

        services.AddSingleton<StaticPathsMethodHandlerFactory>();
        services.AddSingleton<StaticPageRenderer>();
        services.AddSingleton<StaticPageFilter>();
        services.AddSingleton<DebugStaticPageExecutionHandler>();

        services.AddScoped<StaticPageExecutionHandlerAccessor>();

        //plugin into default razor pages logic
        services.Decorate<IPageApplicationModelPartsProvider, StaticPageApplicationModelPartsProvider>();
        services.Decorate<IPageApplicationModelProvider, StaticPageApplicationModelProvider>();

        services.AddTransient<IExportTask, ExportStaticPages>();
    }
}