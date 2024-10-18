using System.Collections.Concurrent;
using Fluid;
using Fluid.Ast;
using Fluid.Parser;
using Fluid.ViewEngine;
using Microsoft.Extensions.FileProviders;

namespace dotnet_api_plus_htmx.Liquid;

public class CustomFluidViewRenderer(FluidViewEngineOptions fluidViewEngineOptions)
    : FluidViewRenderer(fluidViewEngineOptions)
{
    private readonly FluidViewEngineOptions _fluidViewEngineOptions = fluidViewEngineOptions;
    public string? BlockName { get; set; }
    public bool Boosted { get; set; }

    private class CacheEntry
    {
        public IDisposable? Callback;
        public ConcurrentDictionary<string, IFluidTemplate> TemplateCache = new();
    }

    private readonly ConcurrentDictionary<IFileProvider, CacheEntry> _cache = new();

    public override async Task RenderViewAsync(TextWriter writer, string relativePath, TemplateContext context)
    {
        // Provide some services to all statements
        context.AmbientValues[Constants.ViewPathIndex] = relativePath;
        context.AmbientValues[Constants.SectionsIndex] = null; // it is lazily initialized when first used
        context.AmbientValues[Constants.RendererIndex] = this;

        var template = await GetFluidTemplateAsync(relativePath, _fluidViewEngineOptions.ViewsFileProvider, true);

        if (_fluidViewEngineOptions.RenderingViewAsync != null)
        {
            await _fluidViewEngineOptions.RenderingViewAsync.Invoke(relativePath, context);
        }

        // The body is rendered and buffered before the Layout since it can contain fragments 
        // that need to be rendered as part of the Layout.
        // Also the body or its _ViewStarts might contain a Layout tag.
        // The context is not isolated such that variables can be changed by views

        var body = await template.RenderAsync(context, _fluidViewEngineOptions.TextEncoder, isolateContext: false);

        // If a layout is specified while rendering a view, execute it
        if (!Boosted 
            && context.AmbientValues.TryGetValue(Constants.LayoutIndex, out var layoutPath) 
            && layoutPath is string layoutPathString && !String.IsNullOrEmpty(layoutPathString))
        {
            layoutPathString =
                ResolveLayoutPath(relativePath, layoutPathString, _fluidViewEngineOptions.ViewsFileProvider);

            context.AmbientValues[Constants.ViewPathIndex] = layoutPathString;
            context.AmbientValues[Constants.BodyIndex] = body.Trim();

            // Parse the Layout file but ignore viewstarts
            var layoutTemplate = await GetFluidTemplateAsync(layoutPathString,
                _fluidViewEngineOptions.ViewsFileProvider, includeViewStarts: false);

            await layoutTemplate.RenderAsync(writer, _fluidViewEngineOptions.TextEncoder, context);
        }
        else
        {
            await writer.WriteAsync(body.Trim());
        }
    }

    protected override async ValueTask<IFluidTemplate> ParseLiquidFileAsync(string path, IFileProvider fileProvider,
        bool includeViewStarts)
    {
        var fileInfo = fileProvider.GetFileInfo(path);

        if (!fileInfo.Exists)
        {
            return new FluidTemplate();
        }

        var subTemplates = new List<IFluidTemplate>();

        if (includeViewStarts)
        {
            // Add ViewStart files
            foreach (var viewStartPath in FindViewStarts(path, fileProvider))
            {
                // Redefine the current view path while processing ViewStart files
                var callbackTemplate = new FluidTemplate(new CallbackStatement((_, _, context) =>
                {
                    context.AmbientValues[Constants.ViewPathIndex] = viewStartPath;
                    return new ValueTask<Completion>(Completion.Normal);
                }));

                var viewStartTemplate = await GetFluidTemplateAsync(viewStartPath, fileProvider, false);

                subTemplates.Add(callbackTemplate);
                subTemplates.Add(viewStartTemplate);
            }
        }

        await using var stream = fileInfo.CreateReadStream();
        using var sr = new StreamReader(stream);

        var fileContent = await sr.ReadToEndAsync();

        if (!string.IsNullOrEmpty(BlockName))
        {
            var startBlock = "{% block " + BlockName + " %}";
            var blockStart = fileContent.IndexOf(startBlock, StringComparison.Ordinal);
            var blockEnd = fileContent.IndexOf("{% endblock %}", StringComparison.Ordinal);

            if (blockStart == -1)
                throw new ParseException($"Block {BlockName} not found");
            if (blockEnd == -1)
                throw new ParseException($$"""Block {{BlockName}} not closed, expected {% endblock %}""");

            var startIndex = blockStart + startBlock.Length;
            fileContent = fileContent.Substring(startIndex, blockEnd - startIndex).Trim(['\n', ' ']);
        }

        if (_fluidViewEngineOptions.Parser.TryParse(fileContent, out var template, out var errors))
        {
            subTemplates.Add(template);

            return new CompositeFluidTemplate(subTemplates);
        }

        throw new ParseException(errors);
    }

    protected override async ValueTask<IFluidTemplate> GetFluidTemplateAsync(string path, IFileProvider fileProvider,
        bool includeViewStarts)
    {
        var cache = _cache.GetOrAdd(fileProvider, _ =>
        {
            var cacheEntry = new CacheEntry();

            if (_fluidViewEngineOptions.TrackFileChanges)
            {
                Action<object>? callback = null;

                callback = c =>
                {
                    // The order here is important. We need to take the token and then apply our changes BEFORE
                    // registering. This prevents us from possible having two change updates to process concurrently.
                    //
                    // If the file changes after we take the token, then we'll process the update immediately upon
                    // registering the callback.

                    var entry = (CacheEntry)c;
                    var previousCallBack = entry.Callback;
                    previousCallBack?.Dispose();
                    var token = fileProvider.Watch("**/*" + Constants.ViewExtension);
                    entry.TemplateCache.Clear();
                    entry.Callback = token.RegisterChangeCallback(callback!, c);
                };

                cacheEntry.Callback = fileProvider.Watch("**/*" + Constants.ViewExtension)
                    .RegisterChangeCallback(callback!, cacheEntry);
            }

            return cacheEntry;
        });

        if (cache.TemplateCache.TryGetValue($"{path}.{BlockName}", out var template))
        {
            return template;
        }

        template = await ParseLiquidFileAsync(path, fileProvider, includeViewStarts);

        cache.TemplateCache[$"{path}.{BlockName}"] = template;

        return template;
    }
}