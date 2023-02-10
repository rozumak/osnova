using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Osnova.StaticRazorPages.Infrastructure;

namespace Osnova.StaticRazorPages;

internal class PostConfigureMvcOptions : IPostConfigureOptions<MvcOptions>
{
    private readonly StaticPageFilter _staticPageFilter;

    public PostConfigureMvcOptions(StaticPageFilter staticPageFilter)
    {
        _staticPageFilter = staticPageFilter;
    }

    public void PostConfigure(string name, MvcOptions options)
    {
        if (name != Options.DefaultName)
        {
            return;
        }

        options.Filters.Add(_staticPageFilter);
    }
}