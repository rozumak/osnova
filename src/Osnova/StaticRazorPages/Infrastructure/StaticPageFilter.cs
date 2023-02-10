using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace Osnova.StaticRazorPages.Infrastructure;

public class StaticPageFilter : IAsyncPageFilter
{
    public async Task OnPageHandlerSelectionAsync(PageHandlerSelectedContext context)
    {
        if (!IsOnGetStaticMethod(context.HandlerMethod))
        {
            //this is not our case doing nothing
            return;
        }

        var handler = GetExecutionHandler(context.HttpContext);
        await handler.OnBeforeModelBindingAsync(context.ActionDescriptor,
            context.HandlerInstance, context.RouteData.Values);
    }

    public async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context,
        PageHandlerExecutionDelegate next)
    {
        if (!IsOnGetStaticMethod(context.HandlerMethod))
        {
            //it's not our static page handler call regular delegate
            await next.Invoke();
            return;
        }

        var handler = GetExecutionHandler(context.HttpContext);
        await handler.OnPageHandlerExecutionAsync(context.HandlerInstance, next);
    }

    private bool IsOnGetStaticMethod(HandlerMethodDescriptor? methodDescriptor)
    {
        return methodDescriptor?.MethodInfo.Name
            .Equals(StaticPageMethodNames.OnGetStatic, StringComparison.Ordinal) == true;
    }

    private IStaticPageExecutionHandler GetExecutionHandler(HttpContext httpContext)
    {
        var accessor = httpContext.RequestServices.GetRequiredService<StaticPageExecutionHandlerAccessor>();
        return accessor.ExecutionHandler;
    }
}