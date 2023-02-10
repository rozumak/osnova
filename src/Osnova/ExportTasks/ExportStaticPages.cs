using System.Reflection;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Osnova.StaticRazorPages;
using Osnova.StaticRazorPages.Infrastructure;

namespace Osnova.ExportTasks
{
    public class ExportStaticPages : IExportTask
    {
        private readonly ILogger<ExportStaticPages> _logger;

        private readonly IEnumerable<EndpointDataSource> _endpointData;
        private readonly StaticPageDescriptorRegistry _staticPagesRegistry;
        private readonly StaticPageRenderer _staticPageRenderer;

        public string Name => "Export StaticRazorPages";

        public ExportStaticPages(ILogger<ExportStaticPages> logger,
            IEnumerable<EndpointDataSource> endpointData,
            StaticPageDescriptorRegistry staticPagesRegistry,
            StaticPageRenderer staticPageRenderer)
        {
            _logger = logger;
            _endpointData = endpointData;
            _staticPagesRegistry = staticPagesRegistry;
            _staticPageRenderer = staticPageRenderer;
        }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            var endpoints = _endpointData
                .SelectMany(es => es.Endpoints)
                .OfType<RouteEndpoint>();

            var staticPages = MatchRoutesWithStaticPages(endpoints);

            var statistics = new ExecutionStatistics();
            var parallelOptions = new ParallelOptions
            {
                MaxDegreeOfParallelism = 10,
            };
            await Parallel.ForEachAsync(staticPages, parallelOptions,
                async (staticPage, token) => { await _staticPageRenderer.RenderAsync(statistics, staticPage, token); });

            _logger.LogInformation(
                "Rendered pages Total: {TotalPages}, Failed: {FailedPages}, Pages types: {PagesTypes}.",
                statistics.TotalPages, statistics.FailedPages, statistics.PagesTypes);
        }

        private IEnumerable<StaticPage> MatchRoutesWithStaticPages(IEnumerable<RouteEndpoint> pagesEndpoints)
        {
            var result = new Dictionary<TypeInfo, StaticPage>();
            var staticPages = _staticPagesRegistry.StaticPages;
            foreach (var endpoint in pagesEndpoints)
            {
                var pageDescriptor = endpoint.Metadata
                    .OfType<CompiledPageActionDescriptor>()
                    .FirstOrDefault();

                if (pageDescriptor != null && endpoint.RequestDelegate != null)
                {
                    //get static page for this endpoint
                    if (staticPages.TryGetValue(pageDescriptor.PageTypeInfo, out var staticPageDescriptor))
                    {
                        if (!result.TryGetValue(pageDescriptor.PageTypeInfo, out StaticPage? staticPage))
                        {
                            staticPage = new StaticPage(pageDescriptor, staticPageDescriptor);
                            result[pageDescriptor.PageTypeInfo] = staticPage;
                        }

                        staticPage.AddEndpoint(endpoint);
                    }
                }
            }

            return result.Values;
        }
    }
}
