namespace dotnet_api_plus_htmx.Liquid;

public static class CustomResults
{
    public static IResult Block(string viewName, string blockName)
    {
        return new ViewResult(viewName, blockName);
    }

    public static IResult Block(string viewName, string blockName, object model)
    {
        return new ViewResult(viewName, blockName, false, model);
    }
        
    public static IResult View(string viewName, bool boosted = false)
    {
        return new ViewResult(viewName, null, boosted);
    }

    public static IResult View(string viewName, object model, bool boosted = false)
    {
        return new ViewResult(viewName, null, boosted, model);
    }
}