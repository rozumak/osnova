public class Articles
{
    public IReadOnlyList<Article> All { get; } = new List<Article>
    {
        new() {Slug = "article-first", Content = "Hello from first article"},
        new() {Slug = "article-second", Content = "Hello from second article"},
        new() {Slug = "article-third", Content = "Hello from third article"},
    };

    public class Article
    {
        public string Slug { get; set; }

        public string Content { get; set; }
    }
}