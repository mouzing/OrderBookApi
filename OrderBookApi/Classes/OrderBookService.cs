using System.Threading.Channels;

namespace SimpleOrderBookApp.Classes;

public class OrderBookService : IHostedService
{
    private readonly Channel<Order> _channel;
    private readonly OrderBook _orderBook;
    private Task _consumerTask;

    public OrderBookService()
    {
        _channel = Channel.CreateUnbounded<Order>();
        _orderBook = new OrderBook();
    }

    public ValueTask SubmitOrderAsync(Order order) => _channel.Writer.WriteAsync(order);
    
    // Not thread safe but whatever
    public IEnumerable<Order>? GetBestBidOrders() => _orderBook.GetBestBidOrders();
    public IEnumerable<Order>? GetBestAskOrders() => _orderBook.GetBestAskOrders();
    public Dictionary<decimal, Queue<Order>>? GetAllBidOrders() => _orderBook.GetAllBidOrders();
    public Dictionary<decimal, Queue<Order>>? GetAllAskOrders() => _orderBook.GetAllAskOrders();

    // Single consumer — runs on one thread
    private async Task ConsumeAsync()
    {
        await foreach (var order in _channel.Reader.ReadAllAsync())
        {
            _orderBook.SubmitOrder(order); // always single-threaded
        }
    }

    public Task StartAsync(CancellationToken cancellationToken) // Required by IHostedService
    {
        _consumerTask = ConsumeAsync(); // Store the Task so that StopAsync can finish processing 
        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken ct)
    {
        _channel.Writer.Complete(); // signals consumer to finish
        await _consumerTask;
        //await _consumerTask.WaitAsync(ct); // Would wait "some time" (30 seconds is default) before pulling the plug
    }
}