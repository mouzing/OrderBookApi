using SimpleOrderBookApp.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Create a singleton OrderBook instance that lives for the app lifetime
builder.Services.AddSingleton<OrderBook>(new OrderBook());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Get the OrderBook instance
var orderBook = app.Services.GetRequiredService<OrderBook>();

// POST /api/orders - Submit a new order
app.MapPost("/api/orders", (Order order) =>
{
    var success = orderBook.SubmitOrder(order);
    return success ? Results.Ok("Order submitted") : Results.BadRequest("Order failed");
});

// GET /api/orders/best-bid - Get best bid orders
app.MapGet("/api/orders/best-bid", () =>
{
    var bestBids = orderBook.GetBestBidOrders();
    return bestBids != null ? Results.Ok(bestBids) : Results.NotFound("No bids available");
});

// GET /api/orders/best-ask - Get best ask orders
app.MapGet("/api/orders/best-ask", () =>
{
    var bestAsks = orderBook.GetBestAskOrders();
    return bestAsks != null ? Results.Ok(bestAsks) : Results.NotFound("No asks available");
});

// GET /api/orders/all-bid - Get best bid orders
app.MapGet("/api/orders/all-bid", () =>
{
    var bestBids = orderBook.GetAllBidOrders();
    return bestBids != null ? Results.Ok(bestBids) : Results.NotFound("No bids available");
});

// GET /api/orders/all-ask - Get best ask orders
app.MapGet("/api/orders/all-ask", () =>
{
    var bestAsks = orderBook.GetAllAskOrders();
    return bestAsks != null ? Results.Ok(bestAsks) : Results.NotFound("No asks available");
});
// var summaries = new[]
// {
//     "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
// };
// 
// app.MapGet("/weatherforecast", () =>
//     {
//         var forecast = Enumerable.Range(1, 5).Select(index =>
//                 new WeatherForecast
//                 (
//                     DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
//                     Random.Shared.Next(-20, 55),
//                     summaries[Random.Shared.Next(summaries.Length)]
//                 ))
//             .ToArray();
//         return forecast;
//     })
//     .WithName("GetWeatherForecast");

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}