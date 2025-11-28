namespace KafkaWebApi.Features.Orders.ProcessOrder;

public interface IOrderProcessor
{
    Task ProcessAsync(Order order, CancellationToken cancellationToken = default);
}

public class OrderProcessor(ILogger<OrderProcessor> logger) : IOrderProcessor
{
    public async Task ProcessAsync(Order order, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Processing order | OrderId: {OrderId} | Customer: {CustomerId}", 
            order.Id, order.CustomerId);
        
        await SimulateProcessing(cancellationToken);
        
        logger.LogInformation("Order processed successfully | OrderId: {OrderId}", order.Id);
    }

    private static async Task SimulateProcessing(CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
    }

    private static void SimulateRandomFailures(Order order)
    {
        if (Random.Shared.Next(0, 10) > 7)
        {
            throw new InvalidOperationException(
                $"Simulated processing failure for order {order.Id}. " +
                "This could represent inventory unavailable, payment failure, etc.");
        }
    }
}

