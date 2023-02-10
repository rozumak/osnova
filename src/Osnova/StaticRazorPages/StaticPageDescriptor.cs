using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Osnova.StaticRazorPages.Infrastructure;

namespace Osnova.StaticRazorPages;

public class StaticPageDescriptor
{
    public PageApplicationModel PageApplicationModel { get; }

    public PageStaticPathsMethodHandler? GetStaticPathsMethodHandler { get; set; }

    public StaticPageDescriptor(PageApplicationModel pageApplicationModel)
    {
        PageApplicationModel = pageApplicationModel;
    }
}