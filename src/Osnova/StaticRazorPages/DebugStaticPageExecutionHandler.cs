using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace Osnova.StaticRazorPages;

public class DebugStaticPageExecutionHandler : IStaticPageExecutionHandler
{
    private readonly StaticPageDescriptorRegistry _staticPageDescriptorRegistry;

    public DebugStaticPageExecutionHandler(StaticPageDescriptorRegistry staticPageDescriptorRegistry)
    {
        _staticPageDescriptorRegistry = staticPageDescriptorRegistry;
    }

    public async Task OnBeforeModelBindingAsync(CompiledPageActionDescriptor actionDescriptor,
        object handlerInstance, RouteValueDictionary routeValues)
    {
        //get static page descriptor from registry it should be always present
        if (!_staticPageDescriptorRegistry.StaticPages.TryGetValue(
                actionDescriptor.PageTypeInfo, out var descriptor))
        {
            //TODO: 
            throw new Exception();
        }

        //if there no export context it's mean we are in development mode and GetStaticPaths method called on every request
        //for debugging purposes, result is ignored
        var pageMethodHandler = descriptor.GetStaticPathsMethodHandler;
        if (pageMethodHandler != null)
        {
            var staticPaths = await pageMethodHandler.Invoke(handlerInstance);
            _ = new StaticPathsResult(staticPaths);
        }
    }

    public async Task OnPageHandlerExecutionAsync(object handlerInstance, PageHandlerExecutionDelegate next)
    {
        await next.Invoke();
    }
}