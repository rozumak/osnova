using System.Net.Http.Headers;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Osnova.FileStorages;

namespace Osnova.StaticRazorPages.Infrastructure;

public class StaticPageRenderer
{
    private readonly ILogger<StaticPageRenderer> _logger;

    private readonly IContentTypeFileExtensionProvider _fileExtensionProvider;
    private readonly IFileStorage _fileStorage;
    private readonly IHttpContextFactory _httpContextFactory;
    private readonly LinkGenerator _linkGenerator;

    public StaticPageRenderer(ILogger<StaticPageRenderer> logger, IFileStorage fileStorage,
        IContentTypeFileExtensionProvider fileExtensionProvider,
        IHttpContextFactory httpContextFactory, LinkGenerator linkGenerator)
    {
        _logger = logger;
        _httpContextFactory = httpContextFactory;
        _linkGenerator = linkGenerator;
        _fileStorage = fileStorage;
        _fileExtensionProvider = fileExtensionProvider;
    }

    public async Task RenderAsync(ExecutionStatistics statistics, StaticPage staticPage,
        CancellationToken cancellationToken)
    {
        statistics.MarkPageType();

        RouteEndpoint endpoint = SelectEndpoint(staticPage);

        var getStaticPathsHandler = staticPage.StaticPageDescriptor.GetStaticPathsMethodHandler;
        if (getStaticPathsHandler != null)
        {
            var staticPaths = await SafeGetPageStaticPaths(staticPage.StaticPageDescriptor, endpoint);
            if (staticPaths == null)
            {
                statistics.MarkFailed();
                return;
            }

            foreach (var routeValues in staticPaths)
            {
                cancellationToken.ThrowIfCancellationRequested();

                await TryRenderPageAsync(statistics, staticPage.StaticPageDescriptor, endpoint, routeValues);
            }
        }
        else
        {
            await TryRenderPageAsync(statistics, staticPage.StaticPageDescriptor, endpoint);
        }
    }

    private RouteEndpoint SelectEndpoint(StaticPage staticPage)
    {
        var endpoints = staticPage.Endpoints;
        if (endpoints.Count == 1)
        {
            return endpoints[0];
        }

        var orderedEndpoints = endpoints
            .OrderBy(x => x.RoutePattern.PathSegments.Count)
            .ToArray();

        //single page can have more than 1 endpoint with optional index name in path. For example {path}/Index or {path}
        //check if it's points to the same page using optional index notation
        if (endpoints.Count == 2
            && endpoints[0].DisplayName == endpoints[1].DisplayName)
        {
            var lastEndpoint = orderedEndpoints[^1];

            var pathSegments = lastEndpoint.RoutePattern.PathSegments;
            var lastSegment = pathSegments.Where(x => x.IsSimple)
                .SelectMany(x => x.Parts.Where(p => p.IsLiteral).OfType<RoutePatternLiteralPart>())
                .Last();

            if (lastSegment.Content.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                //return endpoint without index part
                return orderedEndpoints[0];
            }
        }

        //TODO: can we get here?
        var result = orderedEndpoints[0];

        _logger.LogWarning(
            "Page {StaticPage} has {Endpoints} endpoints, take {Endpoint} because lowest number of segments.",
            staticPage.PageDescriptor.RelativePath, endpoints.Count, result);
        return result;
    }

    private async Task<IReadOnlyList<RouteValueDictionary>?> SafeGetPageStaticPaths(StaticPageDescriptor descriptor,
        RouteEndpoint endpoint)
    {
        var executionHandler = new GetStaticPathsExecutionHandler(descriptor.GetStaticPathsMethodHandler!);
        using var context = CreateContext(executionHandler, descriptor, endpoint, null);

        try
        {
            await endpoint.RequestDelegate!.Invoke(context.HttpContext);
            return executionHandler.Result!.Paths;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to retrieve StaticPaths {StaticPage} with message '{ExceptionMessage}'.",
                context, e.Message);
            return null;
        }
    }

    private async Task TryRenderPageAsync(ExecutionStatistics statistics, StaticPageDescriptor descriptor,
        RouteEndpoint endpoint, RouteValueDictionary? routeValues = null)
    {
        //TODO: brake loop or add more info into log when what is rendering
        bool hasMissingParameters = ValidateStaticPath(endpoint, routeValues);
        if (hasMissingParameters)
        {
            _logger.LogWarning("Skip {StaticPage} because of missing required static path parameters.",
                descriptor.PageApplicationModel.RelativePath);

            statistics.MarkFailed();
            return;
        }

        var executionHandler = new RenderPageExecutionHandler();
        using var renderContext = CreateContext(executionHandler, descriptor, endpoint, routeValues);

        try
        {
            var response = renderContext.HttpContext.Response;
            response.Body = new MemoryStream();

            await endpoint.RequestDelegate!.Invoke(renderContext.HttpContext);

            if (response.StatusCode != StatusCodes.Status200OK)
            {
                _logger.LogWarning("Skipping {StaticPage} because of Status code {StatusCode}.", renderContext,
                    response.StatusCode);
                return;
            }

            var dest = GetStoragePaths(endpoint, renderContext.HttpContext);
            await StoreRenderResult(response.Body, dest.pageName, dest.path);

            _logger.LogDebug("Rendered {StaticPage} successfully.", renderContext);
            statistics.MarkPage();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to render {StaticPage} with message '{ExceptionMessage}'.", renderContext,
                e.Message);
            statistics.MarkFailed();
        }
    }

    private static bool ValidateStaticPath(RouteEndpoint endpoint, RouteValueDictionary? routeValues)
    {
        //TODO: validate on parameter type match?
        //if route pattern has required values GetStaticPaths method is required
        var pathRequiredParams = endpoint.RoutePattern
            .Parameters
            .Where(x => !x.IsOptional);

        bool hasMissingParameters = false;
        List<RoutePatternParameterPart>? missingParams = null;
        foreach (var pathRequiredParam in pathRequiredParams)
        {
            //there no static paths provided don't render this page
            if (routeValues == null)
            {
                hasMissingParameters = true;
                break;
            }

            if (!routeValues.TryGetValue(pathRequiredParam.Name, out _))
            {
                hasMissingParameters = true;

                missingParams ??= new List<RoutePatternParameterPart>();
                missingParams.Add(pathRequiredParam);
            }
        }

        return hasMissingParameters;
    }

    private async Task StoreRenderResult(Stream pageStream, string pageName, string? path)
    {
        //translate page path to folderPath
        string? folderPath = path;
        if (folderPath != null)
        {
            folderPath = folderPath.Replace('/', Path.DirectorySeparatorChar).ToLowerInvariant();
        }

        await using var fileStream = _fileStorage
            .GetDirectory(folderPath ?? "")
            .CreateWriteStream(pageName);

        pageStream.Position = 0;
        await pageStream.CopyToAsync(fileStream);
    }

    private (string? path, string pageName) GetStoragePaths(RouteEndpoint endpoint, HttpContext httpContext)
    {
        if (!_fileExtensionProvider.TryGetFileExtension(httpContext.Response.ContentType, out string? extension))
        {
            throw new Exception($"Unknown output file content type '{httpContext.Response.ContentType}'.");
        }

        bool isHtmlContent = extension!.Equals(".html", StringComparison.OrdinalIgnoreCase);

        var pathRouteValues = httpContext.GetRouteData().Values;
        var pathSegments = endpoint.RoutePattern.PathSegments;

        StringBuilder? pagePathBuilder = null;
        string? pathPart = null;

        for (var index = 0; index < pathSegments.Count; index++)
        {
            var routePatternPathSegment = pathSegments[index];
            pathPart = routePatternPathSegment.ToPathString(pathRouteValues);

            //if not last segment
            if (index != pathSegments.Count - 1)
            {
                pagePathBuilder ??= new StringBuilder();

                if (index > 0)
                {
                    pagePathBuilder.Append('/');
                }

                pagePathBuilder.Append(pathPart);
            }
        }

        if (pathPart == null)
        {
            return (null, isHtmlContent ? "index.html" : $"default{extension}");
        }

        string? pageName = null;
        if (isHtmlContent)
        {
            //special case for /404 not found page
            if (pathPart.Equals("404", StringComparison.OrdinalIgnoreCase) && pagePathBuilder == null)
            {
                return (null, "404.html");
            }

            if (!pathPart.Equals("index", StringComparison.OrdinalIgnoreCase))
            {
                if (pagePathBuilder == null)
                {
                    pagePathBuilder = new StringBuilder(pathPart);
                }
                else
                {
                    pagePathBuilder.Append('/');
                    pagePathBuilder.Append(pathPart);
                }
            }

            pageName = "index.html";
        }
        else
        {
            var responseHeaders = httpContext.Response.Headers;
            foreach (var contentDispositionRaw in responseHeaders.ContentDisposition)
            {
                var contentDisposition = ContentDispositionHeaderValue.Parse(contentDispositionRaw);
                if (contentDisposition.FileName != null)
                {
                    pageName = contentDisposition.FileName;
                    break;
                }
            }

            pageName ??= $"{pathPart.ToLowerInvariant()}{extension}";
        }

        return (pagePathBuilder?.ToString(), pageName);
    }

    //TODO: extract in factory?
    private RenderPageContext CreateContext(IStaticPageExecutionHandler executionHandler,
        StaticPageDescriptor descriptor, RouteEndpoint endpoint, RouteValueDictionary? routeValues)
    {
        //set page route name, it's needed for framework link generation
        routeValues ??= new RouteValueDictionary();
        routeValues["page"] = endpoint.DisplayName;

        var context = new RenderPageContext(_httpContextFactory, descriptor, endpoint, routeValues);
        var httpContext = context.HttpContext;

        var accessor = httpContext.RequestServices.GetRequiredService<StaticPageExecutionHandlerAccessor>();
        accessor.ExecutionHandler = executionHandler;

        httpContext.SetEndpoint(endpoint);

        //TODO: match more accurately with real http request?
        var request = httpContext.Request;
        request.Method = "GET";
        request.Path = _linkGenerator.GetPathByRouteValues(null, routeValues);
        
        return context;
    }

    private class RenderPageContext : IDisposable
    {
        private readonly IHttpContextFactory _httpContextFactory;

        public StaticPageDescriptor Descriptor { get; }

        public RouteEndpoint Endpoint { get; }

        public HttpContext HttpContext { get; }

        public RenderPageContext(IHttpContextFactory httpContextFactory,
            StaticPageDescriptor descriptor, RouteEndpoint endpoint, RouteValueDictionary routeValues)
        {
            _httpContextFactory = httpContextFactory;

            Descriptor = descriptor;
            Endpoint = endpoint;

            HttpContext = _httpContextFactory.Create(CreateDefaultFeatures());

            var request = HttpContext.Request;
            foreach (var routeValue in routeValues)
            {
                request.RouteValues.TryAdd(routeValue.Key, routeValue.Value);
            }
        }


        public override string ToString()
        {
            var routeValues = HttpContext.Request.RouteValues.Select(x => $"{x.Key}:{x.Value}");
            string routeValuesString = string.Join(", ", routeValues);
            return
                $"[Page = {Descriptor.PageApplicationModel.RelativePath}, Endpoint = {Endpoint}, RouteValues = {{{routeValuesString}}}]";
        }

        public void Dispose()
        {
            _httpContextFactory.Dispose(HttpContext);
        }

        private FeatureCollection CreateDefaultFeatures()
        {
            var features = new FeatureCollection();
            features.Set<IHttpRequestFeature>(new HttpRequestFeature());
            features.Set<IHttpResponseFeature>(new HttpResponseFeature());
            features.Set<IHttpResponseBodyFeature>(new StreamResponseBodyFeature(Stream.Null));
            return features;
        }
    }

    private class GetStaticPathsExecutionHandler : IStaticPageExecutionHandler
    {
        private readonly PageStaticPathsMethodHandler _getStaticPathsMethodHandler;

        public StaticPathsResult? Result { get; private set; }

        public GetStaticPathsExecutionHandler(PageStaticPathsMethodHandler getStaticPathsMethodHandler)
        {
            _getStaticPathsMethodHandler = getStaticPathsMethodHandler;
        }

        public async Task OnBeforeModelBindingAsync(CompiledPageActionDescriptor actionDescriptor,
            object handlerInstance, RouteValueDictionary routeValues)
        {
            StaticPaths resultStaticPaths = await _getStaticPathsMethodHandler.Invoke(handlerInstance);
            Result = new StaticPathsResult(resultStaticPaths);
        }

        public Task OnPageHandlerExecutionAsync(object handlerInstance, PageHandlerExecutionDelegate next)
            => Task.CompletedTask;
    }

    private class RenderPageExecutionHandler : IStaticPageExecutionHandler
    {
        public Task OnBeforeModelBindingAsync(CompiledPageActionDescriptor actionDescriptor,
            object handlerInstance, RouteValueDictionary routeValues)
        {
            return Task.CompletedTask;
        }

        public async Task OnPageHandlerExecutionAsync(object handlerInstance, PageHandlerExecutionDelegate next)
        {
            await next.Invoke();
        }
    }
}