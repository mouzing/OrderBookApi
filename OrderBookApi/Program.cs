using SimpleOrderBookApp.Classes;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Create a singleton OrderBook instance that lives for the app lifetime
//builder.Services.AddSingleton<OrderBook>(new OrderBook());
builder.Services.AddSingleton<OrderBookService>();
builder.Services.AddHostedService(sp => sp.GetRequiredService<OrderBookService>());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Get the OrderBook instance
var orderBookService = app.Services.GetRequiredService<OrderBookService>();

// POST /api/orders - Submit a new order
app.MapPost("/api/orders", async (Order order) =>
{
    await orderBookService.SubmitOrderAsync(order);
    return Results.Accepted("Order submitted");
});

// GET /api/orders/best-bid - Get best bid orders
app.MapGet("/api/orders/best-bid", () =>
{
    var bestBids = orderBookService.GetBestBidOrders();
    return bestBids != null ? Results.Ok(bestBids) : Results.NotFound("No bids available");
});

// GET /api/orders/best-ask - Get best ask orders
app.MapGet("/api/orders/best-ask", () =>
{
    var bestAsks = orderBookService.GetBestAskOrders();
    return bestAsks != null ? Results.Ok(bestAsks) : Results.NotFound("No asks available");
});

// GET /api/orders/all-bid - Get best bid orders
app.MapGet("/api/orders/all-bid", () =>
{
    var bestBids = orderBookService.GetAllBidOrders();
    return bestBids != null ? Results.Ok(bestBids) : Results.NotFound("No bids available");
});

// GET /api/orders/all-ask - Get best ask orders
app.MapGet("/api/orders/all-ask", () =>
{
    var bestAsks = orderBookService.GetAllAskOrders();
    return bestAsks != null ? Results.Ok(bestAsks) : Results.NotFound("No asks available");
});

app.Run();