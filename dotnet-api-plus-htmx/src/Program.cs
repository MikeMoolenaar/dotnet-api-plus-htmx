using dotnet_api_plus_htmx;
using dotnet_api_plus_htmx.Liquid;
using dotnet_api_plus_htmx.routes;
using Fluid;
using Fluid.Ast;
using Fluid.ViewEngine;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAntiforgery();
builder.Services.AddFluid(options =>
{
    options.PartialsFileProvider = new FileProviderMapper(builder.Environment.ContentRootFileProvider, "views");
    options.ViewsFileProvider = new FileProviderMapper(builder.Environment.ContentRootFileProvider, "views");

    options.Parser.RegisterIdentifierBlock("block", async (_, statements, writer, encoder, context) =>
    {
        await statements.RenderStatementsAsync(writer, encoder, context);
        return Completion.Normal;
    });
});
builder.Services.AddSingleton<CustomFluidViewRenderer>(sp =>
{
    var options = sp.GetRequiredService<IOptions<FluidViewEngineOptions>>().Value;
    return new CustomFluidViewRenderer(options);
});
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddSqlite<AppDbContext>(connectionString);


var app = builder.Build();
app.Logger.LogInformation("Using db connection string: {0}", connectionString);
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/static",
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "static")),
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".js"))
        {
            // Cache static js files for a year, because the version is in the filename
            ctx.Context.Response.Headers.Append("Cache-Control", "public, max-age=31536000, immutable");
        }
    }
});

app.UseAntiforgery();
app.MapIndexRoutes();
app.Run();

public record Todo(int Id, string Name, bool Done);