using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace Osnova.StaticRazorPages;

public interface IStaticPageExecutionHandler
{
    Task OnBeforeModelBindingAsync(CompiledPageActionDescriptor actionDescriptor,
        object handlerInstance, RouteValueDictionary routeValues);

    Task OnPageHandlerExecutionAsync(object handlerInstance, PageHandlerExecutionDelegate next);
}