# Osnova

A minimalistic framework for making static websites with ASP.NET Core and Razor Pages. It lets developers use their existing Razor Pages skills to quickly turn their web apps into static sites, without being limited by predefined templates.

## Getting Started

TODO

## Static Razor Pages

### OnGetStatic

Only if your `PageModel` contains a handler method called `OnGetStatic`, Osnova will render such page to output during export with data returned by handler.

```csharp
public class IndexModel : PageModel
{
    public string Message { get; set; }

    public void OnGetStatic()
    {
        Message = "Hello static message!";
    }
}
```

It is equivalent to `OnGet` method handler and can have routing parameters as arguments. In the same way `OnGetStatic` must be public and can return `void`, `Task` if asynchronous, or an `IActionResult` (or `Task<IActionResult>`).
You cannot have both `OnGet` and `OnGetStatic` on the same page.

### GetStaticPaths

Routes with parameters in Razor Pages that use OnGetStatic handler must include a list of paths to be statically generated. Osnova will statically render page with all the paths specified by GetStaticPaths.

```csharp
// @page "/article/{id}"

public class ArticleModel : PageModel
{
    public string ArticleContent { get; set; }

    // Generates `/article/1` and `/article/2`
    public StaticPaths GetStaticPaths()
    {
        return new StaticPaths
        {
            Paths = new []
            {
                new {id = 1}, 
                new {id = 2},
            }
        };
    }

    // Use of `GetStaticPaths` requires using `OnGetStatic`
    public void OnGetStatic(int id)
    {
        // Load article content with passed id as route param
        // ArticleContent = _articlesService.LoadArticleContentById(id);
    }
}
```
