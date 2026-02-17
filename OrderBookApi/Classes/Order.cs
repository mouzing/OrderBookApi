using SimpleOrderBookApp.Enums;

namespace SimpleOrderBookApp.Classes;

public class Order
{   
    public long OrderId { get; set; }
    public OrderType OrderType { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public DateTime OrderDate { get; set; } 
    public decimal TotalPrice => Price * Quantity;

    // TODO: ensure that this is performant
    // This will get used repeatedly. Speed improvements will be nice
    public static Order CreateRandomOrder(int orderId, decimal basePrice, decimal priceVariance, int minQuantity, int maxQuantity)
    {   
        OrderType[] orderTypes = [OrderType.Buy,OrderType.Sell];
        
        var index = Random.Shared.Next(0, orderTypes.Length);
        
        var orderType = orderTypes[index];

        var quantity = Random.Shared.Next(minQuantity, maxQuantity + 1);

        var min = basePrice - priceVariance;
        var max = basePrice + priceVariance;
        
        var orderPrice = min + (decimal)Random.Shared.NextDouble() * (max - min);
        orderPrice = Math.Truncate(orderPrice * 100) / 100;
        
        //Console.WriteLine($"new {orderType.ToString()} order for {quantity} at {orderPrice}");
        
        return new Order()
        {
            OrderId =  orderId,
            OrderType = orderType,
            Price = orderPrice,
            Quantity = quantity,
            OrderDate = DateTime.UtcNow
        };
    } 
    
    // change
}