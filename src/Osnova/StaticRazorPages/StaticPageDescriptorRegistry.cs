namespace Osnova.StaticRazorPages;

public class StaticPageDescriptorRegistry
{
    private readonly Dictionary<Type, StaticPageDescriptor> _staticPages = new();

    public IDictionary<Type, StaticPageDescriptor> StaticPages => _staticPages;

    public void RegisterStaticPage(StaticPageDescriptor staticPage)
    {
        _staticPages[staticPage.PageApplicationModel.PageType] = staticPage;
    }
}