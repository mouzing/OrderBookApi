using System.Threading.Channels;

namespace SimpleOrderBookApp.Classes;

public class OrderBookService : IHostedService
{
    private readonly Channel<Order> _channel;
    private readonly OrderBook _orderBook;

    public OrderBookService()
    {
        _channel = Channel.CreateUnbounded<Order>();
        _orderBook = new OrderBook();
    }

    // Producers call this (controllers, websocket handlers, etc.)
    public ValueTask SubmitOrderAsync(Order order) => _channel.Writer.WriteAsync(order);
    
    // Not thread safe but whatever
    public IEnumerable<Order>? GetBestBidOrders() => _orderBook.GetBestBidOrders();
    public IEnumerable<Order>? GetBestAskOrders() => _orderBook.GetBestAskOrders();
    public Dictionary<decimal, Queue<Order>>? GetAllBidOrders() => _orderBook.GetAllBidOrders();
    public Dictionary<decimal, Queue<Order>>? GetAllAskOrders() => _orderBook.GetAllAskOrders();

    // Single consumer — runs on one thread
    private async Task ConsumeAsync(CancellationToken ct)
    {
        await foreach (var order in _channel.Reader.ReadAllAsync(ct))
        {
            _orderBook.SubmitOrder(order); // always single-threaded
        }
    }

    public Task StartAsync(CancellationToken ct)
    {
        _ = ConsumeAsync(ct); // fire and forget the consumer loop
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken ct)
    {
        _channel.Writer.Complete(); // signals consumer to finish
        return Task.CompletedTask;
    }
}