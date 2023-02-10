using Microsoft.AspNetCore.Mvc.RazorPages;
using Osnova.StaticRazorPages;

namespace WebSample.Pages
{
    public class ArticleModel : PageModel
    {
        private readonly Articles _articles;

        public string Content { get; set; }

        public ArticleModel(Articles articles)
        {
            _articles = articles;
        }

        public void OnGetStatic(string slug)
        {
            var article = _articles.All.Single(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase));
            Content = article.Content;
        }

        public StaticPaths GetStaticPaths()
        {
            return new StaticPaths
            {
                Paths = _articles.All.Select(x => new {slug = x.Slug})
            };
        }
    }
}