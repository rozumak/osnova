using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;

namespace Osnova.StaticRazorPages;

[DebuggerDisplay("{DebuggerDisplayString}")]
public class StaticPage
{
    private readonly List<RouteEndpoint> _endpoints = new();

    public IReadOnlyList<RouteEndpoint> Endpoints => _endpoints;

    public CompiledPageActionDescriptor PageDescriptor { get; }

    public StaticPageDescriptor StaticPageDescriptor { get; }

    public StaticPage(CompiledPageActionDescriptor pageDescriptor, StaticPageDescriptor staticPageDescriptor)
    {
        PageDescriptor = pageDescriptor;
        StaticPageDescriptor = staticPageDescriptor;
    }

    public void AddEndpoint(RouteEndpoint endpoint)
    {
        _endpoints.Add(endpoint);
    }

    private string DebuggerDisplayString => $"{nameof(PageDescriptor.ViewEnginePath)} = {PageDescriptor.ViewEnginePath}, {nameof(PageDescriptor.RelativePath)} = {PageDescriptor.RelativePath}";
}