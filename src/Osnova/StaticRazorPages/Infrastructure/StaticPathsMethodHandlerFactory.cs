using System.Reflection;
using System.Runtime.ExceptionServices;

namespace Osnova.StaticRazorPages.Infrastructure;

public class StaticPathsMethodHandlerFactory
{
    public PageStaticPathsMethodHandler Create(MethodInfo methodInfo)
    {
        if (methodInfo.GetParameters().Length == 0)
        {
            if (methodInfo.ReturnType == typeof(Task<StaticPaths>))
                return new AsyncHandlerMethod(methodInfo);

            if (methodInfo.ReturnType == typeof(StaticPaths))
                return new HandlerMethod(methodInfo);
        }

        throw new InvalidOperationException("GetStaticPathsMethod has invalid signature.");
    }

    private class HandlerMethod : PageStaticPathsMethodHandler
    {
        private readonly Func<object, StaticPaths> _executor;

        public HandlerMethod(MethodInfo methodInfo)
        {
            _executor = (instance) =>
            {
                try
                {
                    return (StaticPaths)methodInfo.Invoke(instance, null)!;
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    }

                    throw;
                }
            };
        }

        public override Task<StaticPaths> Invoke(object pageModelInstance)
        {
            return Task.FromResult(_executor(pageModelInstance));
        }
    }

    private class AsyncHandlerMethod : PageStaticPathsMethodHandler
    {
        private readonly Func<object, Task<StaticPaths>> _executor;

        public AsyncHandlerMethod(MethodInfo methodInfo)
        {
            _executor = (instance) =>
            {
                try
                {
                    return (Task<StaticPaths>)methodInfo.Invoke(instance, null)!;
                }
                catch (TargetInvocationException ex)
                {
                    if (ex.InnerException != null)
                    {
                        ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                    }

                    throw;
                }
            };
        }

        public override Task<StaticPaths> Invoke(object pageModelInstance)
        {
            return _executor(pageModelInstance);
        }
    }
}