using System.Collections.Concurrent;
using Fluid;
using Fluid.ViewEngine;
using Microsoft.Extensions.Options;

// Based on https://github.com/sebastienros/fluid/blob/main/MinimalApis.LiquidViews/ActionViewResult.cs
namespace dotnet_api_plus_htmx.Liquid;

public class ViewResult : IResult
{
    private readonly string _viewName;
    private readonly string? _blockName;
    private readonly bool _boosted;
    private readonly object _model;
    public int StatusCode { get; set; } = 200;

    private static readonly ConcurrentDictionary<string, string> ViewLocationsCache = new();
    private static string ContentType => "text/html";

    public ViewResult(string viewName, string? blockName = null, bool boosted = false, object? model = null)
    {
        _viewName = viewName;
        _blockName = blockName;
        _boosted = boosted;
        _model = model ?? new object();
    }

    public ViewResult WithResponseCode(int statusCode)
    {
        StatusCode = statusCode;
        return this;
    }

    public async Task ExecuteAsync(HttpContext httpContext)
    {
        var fluidViewRenderer = httpContext.RequestServices.GetRequiredService<CustomFluidViewRenderer>();
        fluidViewRenderer.BlockName = _blockName;
        fluidViewRenderer.Boosted = _boosted;
        var options = httpContext.RequestServices.GetRequiredService<IOptions<FluidViewEngineOptions>>().Value;

        string? viewPath = LocatePageFromViewLocations(_viewName, options);

        if (viewPath is null)
        {
            httpContext.Response.StatusCode = 404;
            return;
        }

        httpContext.Response.StatusCode = StatusCode;
        httpContext.Response.ContentType = ContentType;

        var context = new TemplateContext(_model, options.TemplateOptions);
        context.Options.FileProvider = options.PartialsFileProvider;
            
        await using var sw = new StreamWriter(httpContext.Response.Body);
        await fluidViewRenderer.RenderViewAsync(sw, viewPath, context);
    }

    private static string? LocatePageFromViewLocations(string viewName, FluidViewEngineOptions options)
    {
        if (ViewLocationsCache.TryGetValue(viewName, out string? cachedLocation) && cachedLocation != null)
        {
            return cachedLocation;
        }

        var fileProvider = options.ViewsFileProvider;

        foreach (var location in options.ViewsLocationFormats)
        {
            var viewFilename = Path.Combine(String.Format(location, viewName));

            var fileInfo = fileProvider.GetFileInfo(viewFilename);

            if (fileInfo.Exists)
            {
                ViewLocationsCache[viewName] = viewFilename;
                return viewFilename;
            }
        }

        return null;
    }
}