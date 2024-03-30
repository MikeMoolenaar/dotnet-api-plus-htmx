using dotnet_api_plus_htmx.Liquid;
using dotnet_api_plus_htmx.routes;
using Fluid;
using Fluid.Ast;
using Fluid.ViewEngine;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

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


var app = builder.Build();
app.UseStaticFiles(new StaticFileOptions
{
    RequestPath = "/static",
    FileProvider = new PhysicalFileProvider(Path.Combine(app.Environment.ContentRootPath, "static"))
});

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapIndexRoutes();
app.Run();

public record Todo(int Id, string Name, bool Done);