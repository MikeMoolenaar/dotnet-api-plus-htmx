using Microsoft.EntityFrameworkCore;

namespace dotnet_api_plus_htmx;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Todo> Todos { get; init; }
}