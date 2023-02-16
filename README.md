# Osnova

A minimalistic framework for making static websites with ASP.NET Core and Razor Pages. It lets developers use their existing Razor Pages skills to quickly turn their web apps into static sites, without being limited by predefined templates.

## Getting Started

### Step 1: Create your project

Start by creating a new ASP.NET Core Razor Pages project if you don’t have one set up already. Use .NET CLI or follow this [guide](https://learn.microsoft.com/en-us/aspnet/core/tutorials/razor-pages/razor-pages-start).

```
dotnet new razor
```

### Step 2: Install Osnova

Install [Osnova](https://www.nuget.org/packages/Osnova) from the .NET CLI as:

```
dotnet add package Osnova
```

Or from the package manager console:

```
PM> Install-Package Osnova
```

Use the --version option to specify a preview version to install.

### Step 3: Configure your services

To register Osnova services, open `Program.cs`, and add a call to a `builder.Services.AddOsnova()` after `builder.Services.AddRazorPages()` call:

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddOsnova();
```

### Step 4: Start adding static pages to your project

Let's modify default `Pages\Index` page to be static. Replace `OnGet` with `OnGetStatic` method handler.

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

Update `Index.cshtml` template.

```html
@page @model IndexModel @{ ViewData["Title"] = "Home page"; }

<h1>@Model.Message</h1>
```

You can serve the site locally by running app for example with CLI command:

```
dotnet watch run
```

### Step 5: Export static content

To export an application to static files run it with `export` parameter using CLI command:

```
dotnet run -- export
```

This will export the content of `wwwroot` folder and rendered static pages to `output` folder in your project directory.

Congratulations, you are ready to deploy your `Osnova` application to production with the hosting of your choice.

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
