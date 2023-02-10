using Microsoft.AspNetCore.Routing;

namespace Osnova.StaticRazorPages;

internal class StaticPathsResult
{
    private static readonly List<RouteValueDictionary> Empty = new();

    public IReadOnlyList<RouteValueDictionary> Paths { get; }

    public StaticPathsResult(StaticPaths staticPaths)
    {
        Paths = staticPaths.Paths != null
            ? staticPaths.Paths.Select(x => new RouteValueDictionary(x)).ToList()
            : Empty;
    }
}