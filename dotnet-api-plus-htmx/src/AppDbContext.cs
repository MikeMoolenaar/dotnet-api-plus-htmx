using Microsoft.EntityFrameworkCore;

namespace dotnet_api_plus_htmx;

public class AppDbContext(IConfiguration configuration) : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
    }
    
    public DbSet<Todo> Todos { get; set; }
}