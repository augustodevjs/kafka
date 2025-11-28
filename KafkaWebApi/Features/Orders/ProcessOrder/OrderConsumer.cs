using KafkaWebApi.Infrastructure.Kafka.Messaging;

namespace KafkaWebApi.Features.Orders.ProcessOrder;

public class OrderConsumer(
    IConfiguration configuration, 
    ILogger<OrderConsumer> logger,
    IOrderProcessor orderProcessor) 
    : MessageConsumer<Order>(configuration, logger)
{
    protected override string Topic => "orders";
    protected override string ConsumerGroupId => "order-consumer-group";

    protected override async Task HandleMessageAsync(Order order, string key, CancellationToken cancellationToken)
    {
        logger.LogInformation("Order received | OrderId: {OrderId} | Customer: {CustomerId} | Items: {ItemCount}",
            order.Id, order.CustomerId, order.Items.Count);

        foreach (var item in order.Items)
        {
            logger.LogInformation("  - Product: {ProductId} x{Quantity}",
                item.ProductId, item.Quantity);
        }

        await orderProcessor.ProcessAsync(order, cancellationToken);
    }
}

