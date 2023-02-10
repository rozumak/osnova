using System.Reflection;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Osnova.StaticRazorPages.Infrastructure;

namespace Osnova.StaticRazorPages.ApplicationModels;

public class StaticPageApplicationModelProvider : IPageApplicationModelProvider
{
    private readonly IPageApplicationModelProvider _originalPageModelProvider;
    private readonly StaticPageDescriptorRegistry _staticPagesRegistry;
    private readonly StaticPathsMethodHandlerFactory _handlerFactory;

    public int Order => _originalPageModelProvider.Order;

    public StaticPageApplicationModelProvider(IPageApplicationModelProvider originalPageModelProvider,
        StaticPageDescriptorRegistry registry, StaticPathsMethodHandlerFactory handlerFactory)
    {
        _originalPageModelProvider = originalPageModelProvider;
        _staticPagesRegistry = registry;
        _handlerFactory = handlerFactory;
    }

    public void OnProvidersExecuting(PageApplicationModelProviderContext context)
    {
        _originalPageModelProvider.OnProvidersExecuting(context);

        var pageModel = context.PageApplicationModel;
        if (pageModel.HandlerMethods?.Count > 0)
        {
            //TODO: remove only OnGet method if exist
            var staticMethod = pageModel.HandlerMethods
                .FirstOrDefault(x =>
                    x.Name.Equals(StaticPageMethodNames.OnGetStatic, StringComparison.Ordinal));

            if (staticMethod != null)
            {
                pageModel.HandlerMethods.Clear();
                pageModel.HandlerMethods.Add(staticMethod);

                var descriptor = new StaticPageDescriptor(pageModel);

                var getPathsMethod = TryGetStaticPathsHandlerMethod(context);
                if (getPathsMethod != null)
                {
                    descriptor.GetStaticPathsMethodHandler = _handlerFactory.Create(getPathsMethod);
                }

                _staticPagesRegistry.RegisterStaticPage(descriptor);
            }
        }
    }

    public void OnProvidersExecuted(PageApplicationModelProviderContext context)
    {
        _originalPageModelProvider.OnProvidersExecuted(context);
    }

    private MethodInfo? TryGetStaticPathsHandlerMethod(PageApplicationModelProviderContext context)
    {
        var methods = context.PageApplicationModel.HandlerType.GetMethods();
        foreach (var method in methods)
        {
            if (method.Name.Equals(StaticPageMethodNames.GetStaticPaths, StringComparison.Ordinal))
            {
                if (method.GetParameters().Length == 0)
                {
                    bool returnTypeMatched = method.ReturnType == typeof(StaticPaths) ||
                                             method.ReturnType == typeof(Task<StaticPaths>);

                    if (returnTypeMatched)
                    {
                        return method;
                    }
                }
            }
        }

        return null;
    }
}