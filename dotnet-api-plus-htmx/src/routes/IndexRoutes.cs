using dotnet_api_plus_htmx.Liquid;
using Htmx;

namespace dotnet_api_plus_htmx.routes;

public static class IndexRoutes
{
    public static void MapIndexRoutes(this WebApplication app)
    {
        app.MapGet("/", (HttpRequest request) =>
        {
            var model = new Todo(1, "Go back to work!", false);
            return CustomResults.View("Index", model, request.IsHtmxBoosted());
        });
        app.MapGet("/about", (HttpRequest request) => CustomResults.View("About", request.IsHtmxBoosted()));
        app.MapGet("/fragment",
            () =>
            {
                var number = new Random().Next(0, 100);
                var model = new Todo(2, $"Go back to work! <h1>pwnd! {number}</h1>", false);
                return CustomResults.Block("Index", "some-content", model);
            });
    }
}