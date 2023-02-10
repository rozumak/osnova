using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;

namespace Osnova.StaticRazorPages.Infrastructure;

public static class RoutePatternPathSegmentExtensions
{
    public static string ToPathString(this RoutePatternPathSegment segment, RouteValueDictionary? routeValues = null)
    {
        return string.Join(string.Empty, segment.Parts.Select(x => PartAsString(x, routeValues)));
    }

    public static string? PartAsString(this RoutePatternPart part, RouteValueDictionary? routeValues = null)
    {
        return part switch
        {
            RoutePatternLiteralPart literalPart => literalPart.Content,
            RoutePatternSeparatorPart separatorPart => separatorPart.Content,
            RoutePatternParameterPart parameterPart => routeValues != null
                ? routeValues[parameterPart.Name]?.ToString()
                : throw new Exception($"Required path parameter {parameterPart.Name} is missing."),
            _ => throw new NotSupportedException("Part of type is not supported.")
        };
    }
}