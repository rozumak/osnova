using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WebSample.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ILogger<IndexModel> _logger;
        private readonly Articles _articles;

        public List<string> Slugs { get; set; }

        public IndexModel(ILogger<IndexModel> logger, Articles articles)
        {
            _logger = logger;
            _articles = articles;
        }

        public void OnGetStatic()
        {
            Slugs = _articles.All.Select(x => x.Slug).ToList();
        }
    }
}