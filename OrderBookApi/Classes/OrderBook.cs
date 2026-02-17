using SimpleOrderBookApp.Enums;

namespace SimpleOrderBookApp.Classes;

public class OrderBook
{
    public PriorityQueue<Queue<Order>, decimal> BuyOrders { get; set; }
    public Dictionary<decimal, Queue<Order>> BuyOrderLookUpTable { get; set; }
    public PriorityQueue<Queue<Order>, decimal> SellOrders { get; set; }
    public Dictionary<decimal, Queue<Order>> SellOrderLookUpTable { get; set; }
    public bool PrintOrderFills { get; set; }

    public OrderBook(bool printOrderFills=false)
    {
        BuyOrders = new PriorityQueue<Queue<Order>, decimal>();
        BuyOrderLookUpTable = new Dictionary<decimal, Queue<Order>>();
        SellOrders = new PriorityQueue<Queue<Order>, decimal>();
        SellOrderLookUpTable = new Dictionary<decimal, Queue<Order>>();
        PrintOrderFills = printOrderFills;
    }
    
    // when a new order comes in it should be checked against the .peek value of the other queue. if the "matching logic" says its a pair then fill the orders (can be a partial fill)

    public bool SubmitOrder(Order order)
    {
        return order.OrderType switch
        {
            OrderType.Buy => SubmitBuyOrder(order),
            OrderType.Sell => SubmitSellOrder(order),
            _ => false
        };
    }
    
    private bool SubmitBuyOrder(Order order)
    {
        // check the peak of sell order (which is a peek.peek) and see if the buy order price is greater than or equal to that. if so: execute an order. if not enqueue the new buy order (
        
        // TODO: change this to a known stored value
        if (SellOrders.Count > 0)
        {
            while (order.Quantity > 0 && SellOrders.Count > 0) // Not sure if quite right..quantity > 0 or nothing in the selling queue is available at the price level or below
            {
                // TODO: could make this different
                var peekedValue = SellOrders.Peek().Peek().Price;
                
                if (order.Price >= peekedValue)
                {
                    var priceLevelQueueExhausted = ProcessOrderAtPriceLevel(order, SellOrders.Peek());
                    if (priceLevelQueueExhausted)
                    {
                        SellOrders.Dequeue();
                        if (!SellOrderLookUpTable.Remove(peekedValue))
                        {
                            Console.WriteLine($"Price {order.Price} was not found in the sell look up table");
                        }
                    } 
                }
                else
                {
                    // No sell orders below buy order price
                    AddBuyOrder(order);
                    return true;
                }
            }

            if (order.Quantity > 0)
            {
                AddBuyOrder(order);
            }
        }
        else
        {
           AddBuyOrder(order); 
        }
        
        return true;
    }

    private bool SubmitSellOrder(Order order)
    {
        if (BuyOrders.Count > 0)
        {
            while (order.Quantity > 0 && BuyOrders.Count > 0) 
            {
                // TODO: could make this different
                var peekedValue = BuyOrders.Peek().Peek().Price;
                if (order.Price <= peekedValue)
                {
                    var priceLevelQueueExhausted = ProcessOrderAtPriceLevel(order, BuyOrders.Peek());
                    if (priceLevelQueueExhausted)
                    {
                        BuyOrders.Dequeue();
                        if (!BuyOrderLookUpTable.Remove(peekedValue))
                        {
                            Console.WriteLine($"Price {order.Price} was not found in the buy look up table");
                        }
                    } 
                }
                else
                {
                    // No sell orders below buy order price
                    AddSellOrder(order);
                    return true;
                }
            }

            if (order.Quantity > 0)
            {
                AddSellOrder(order);
            }
        }
        else
        {
            AddSellOrder(order);
        }
        
        return true;
    }
    
    public bool AddOrder(Order order)
    {
        return order.OrderType switch
        {
            OrderType.Buy => AddBuyOrder(order),
            OrderType.Sell => AddSellOrder(order),
            _ => false
        };
    }

    private bool AddBuyOrder(Order order)
    {
       var priceLvlQExists = BuyOrderLookUpTable.TryGetValue(order.Price, out var priceLevelBuyOrders);

       priceLevelBuyOrders ??= new Queue<Order>();

       priceLevelBuyOrders.Enqueue(order);

       if (priceLvlQExists) return true;
       
       BuyOrderLookUpTable.Add(order.Price, priceLevelBuyOrders);
       
       BuyOrders.Enqueue(priceLevelBuyOrders, -order.Price);
       
       return true;
    }

    private bool AddSellOrder(Order order)
    {
       var priceLvlQExists = SellOrderLookUpTable.TryGetValue(order.Price, out var priceLevelSellOrders);

       priceLevelSellOrders ??= new Queue<Order>();

       priceLevelSellOrders.Enqueue(order);

       if (priceLvlQExists) return true;
       
       SellOrderLookUpTable.Add(order.Price, priceLevelSellOrders);
       
       SellOrders.Enqueue(priceLevelSellOrders, order.Price);
       
       return true;
    }
    
    private bool ProcessOrderAtPriceLevel(Order order, Queue<Order> orderQueue)
    {       
        while (order.Quantity > 0 && orderQueue.Count > 0)
        {
            var firstOut = orderQueue.Peek();
            if (firstOut.Quantity > order.Quantity)
            {
                LogOrder(order, firstOut.Price, order.Quantity);
                firstOut.Quantity -= order.Quantity;
                // TODO: this is a weird work around..maybe add a filled bool?
                order.Quantity = 0;
                return false;
            }
            
            LogOrder(order, firstOut.Price, firstOut.Quantity); 
            order.Quantity -= firstOut.Quantity;
            orderQueue.Dequeue();
        }

        return orderQueue.Count == 0; 
    }

    private void LogOrder(Order order, decimal filledPrice, int filledQuantity)
    {
        if (!PrintOrderFills) return;
        return;
        
        Console.WriteLine($"Order {order.OrderId} filed {filledQuantity} of {order.Quantity} at ${filledPrice}"); 
    }

    public IEnumerable<Order>? GetBestBidOrders()
    {
        return BuyOrders.Peek();
    }

    public IEnumerable<Order>? GetBestAskOrders()
    {
        return SellOrders.Peek();
    }
}