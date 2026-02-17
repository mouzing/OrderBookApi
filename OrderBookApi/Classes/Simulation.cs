namespace SimpleOrderBookApp.Classes;

public class Simulation
{
    public OrderBook OrderBook;
    private readonly int _tickDurationMs = 500;
    private readonly decimal _basePrice = 100.00m;
    private readonly decimal _priceVariance = 10.0m;
    private readonly int _minQuantity = 1;
    private readonly int _maxQuantity = 100;
    
    private CancellationTokenSource _cancellationTokenSource;
    private int _orderCount = 0;
    
    public Simulation()
    {
        OrderBook = new OrderBook(printOrderFills:true);
    }

    public Simulation(int? tickDuration=null, decimal? basePrice=null, decimal? priceVariance=null, int? minQuantity=null, int? maxQuantity=null)
    {
        OrderBook = new OrderBook(printOrderFills:true);
        
        if (tickDuration != null)
            _tickDurationMs = tickDuration.Value;
        
        if (basePrice != null)
            _basePrice = basePrice.Value;
        
        if (priceVariance != null)
            _priceVariance = priceVariance.Value;
        
        if (minQuantity != null)
            _minQuantity = minQuantity.Value;

        if (maxQuantity != null)
            _maxQuantity = maxQuantity.Value;
    }

    public void Start()
    {
       _cancellationTokenSource = new CancellationTokenSource();
       // TODO: look into how this works. what is it actually doing/is there a better way of achieving the same result. Do I even need it?
       
       Task.Run(() => RunSimulation(_cancellationTokenSource.Token));
    }
    
    private async Task RunSimulation(CancellationToken cancellationToken)
    {
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        long orderCount = 0;
        long tickCount = 0;
        while (!cancellationToken.IsCancellationRequested && stopwatch.ElapsedMilliseconds < 15000)
        {
            // NOTE: this really doesn't slow down the tick operation at all and results in might higher throughput
            // TODO: write ahead log
            // TODO: DB?
            // TODO: UI (winforms or the other one?)
            // TODO: Agents
            // TODO: Have this just act as an order book 
            for (int i = 0; i < 1000; i++)
            {
                OrderBook.SubmitOrder(Order.CreateRandomOrder(_orderCount++, _basePrice, _priceVariance, _minQuantity, _maxQuantity));
                orderCount++;
            }

            Console.WriteLine("done: " + tickCount);
            // TODO: when the UI is present
            /*
             * // Marshal back to UI thread for display
                await Dispatcher.InvokeAsync(() => UpdateOrderBookDisplay(myOrderBook));
             */
            if (tickCount % 100 == 0)
            {
                var avgMs = stopwatch.ElapsedMilliseconds / (double)tickCount;
                Console.WriteLine($"Average tick rate: {avgMs:F2}ms ({1000/avgMs:F1} ticks/sec)");
            }
            tickCount++; 
            await Task.Delay(_tickDurationMs, cancellationToken);
        }

        Console.WriteLine($"Simulation finished in {stopwatch.ElapsedMilliseconds}ms");
        Console.WriteLine(orderCount);
        Console.WriteLine(tickCount);
    }
}