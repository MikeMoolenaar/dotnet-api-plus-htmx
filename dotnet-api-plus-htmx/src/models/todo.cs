namespace dotnet_api_plus_htmx.models;

public class Todo
{
    public required int Id { get; set; }
    public required string Title { get; set; }
    public required bool IsComplete { get; set; }
}