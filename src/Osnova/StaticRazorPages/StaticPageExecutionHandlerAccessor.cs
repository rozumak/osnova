namespace Osnova.StaticRazorPages;

public class StaticPageExecutionHandlerAccessor
{
    public IStaticPageExecutionHandler ExecutionHandler { get; set; }

    public StaticPageExecutionHandlerAccessor(DebugStaticPageExecutionHandler debugExecutionHandler)
    {
        ExecutionHandler = debugExecutionHandler;
    }
}