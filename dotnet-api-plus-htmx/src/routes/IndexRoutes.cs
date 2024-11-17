using dotnet_api_plus_htmx.Liquid;
using Htmx;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace dotnet_api_plus_htmx.routes;

public static class IndexRoutes
{
    record LoginForm(string Email, string Password);
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
        app.MapGet("/todos", async (HttpRequest request, AppDbContext db) =>
        {
            var todos = await db.Todos.ToListAsync();
            return CustomResults.View("Todos", new { todos }, request.IsHtmxBoosted());
        });
        app.MapGet("/login", (HttpContext httpContext, HttpRequest request, IAntiforgery antiforgery) =>
        {
            var csrf = antiforgery.GetAndStoreTokens(httpContext).RequestToken;
            return CustomResults.View("Login", new { csrf }, httpContext.Request.IsHtmxBoosted());
        });
        
        app.MapPost("/login", ([FromForm] LoginForm loginForm, HttpContext httpContext, IAntiforgery antiforgery) =>
        {
            var csrf = antiforgery.GetAndStoreTokens(httpContext).RequestToken;
            var response = new
            {
                form = loginForm,
                errors = new { general = "Email or password incorrect" },
                csrf
            };
            return CustomResults.View("Login", response, boosted:true);
            
        });
        app.MapFallback((HttpRequest request) =>
        {
            if (request.Path.StartsWithSegments("/static"))
                return Results.Text("Not found", statusCode:StatusCodes.Status404NotFound);
            return CustomResults.View("404", request.IsHtmxBoosted())
                .WithResponseCode(StatusCodes.Status404NotFound);
        });
    }
}