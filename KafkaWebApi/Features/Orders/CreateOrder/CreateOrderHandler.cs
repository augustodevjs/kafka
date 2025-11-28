using KafkaWebApi.Infrastructure.Kafka.Messaging;

namespace KafkaWebApi.Features.Orders.CreateOrder;

public interface ICreateOrderHandler
{
    Task<OrderResult> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default);
}

public class CreateOrderHandler(
    IMessagePublisher publisher, 
    ILogger<CreateOrderHandler> logger) : ICreateOrderHandler
{
    private const string OrdersTopic = "orders";

    public async Task<OrderResult> HandleAsync(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Creating order | OrderId: {OrderId} | Customer: {CustomerId} | Items: {ItemCount}",
            command.Id, command.CustomerId, command.Items.Count);

        var order = MapToOrder(command);
        
        await publisher.PublishAsync(OrdersTopic, order.Id, order, cancellationToken: cancellationToken);

        logger.LogInformation("Order published successfully | OrderId: {OrderId}", order.Id);

        return new OrderResult(order.Id, "Order created and queued for processing", order.Items.Count);
    }

    private static Order MapToOrder(CreateOrderCommand command)
    {
        return new Order
        {
            Id = command.Id,
            CustomerId = command.CustomerId,
            Items = command.Items.Select(i => new OrderItem
            {
                ProductId = i.ProductId,
                Quantity = i.Quantity
            }).ToList()
        };
    }
}

