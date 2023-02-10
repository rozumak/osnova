namespace Osnova.StaticRazorPages.Infrastructure;

public abstract class PageStaticPathsMethodHandler
{
    public abstract Task<StaticPaths> Invoke(object pageModelInstance);
}