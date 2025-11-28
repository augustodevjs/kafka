namespace KafkaWebApi.Features.Orders.CreateOrder;

public class CreateOrderCommand
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required List<OrderItemDto> Items { get; init; }
}

public class OrderItemDto
{
    public required string ProductId { get; init; }
    public int Quantity { get; init; }
}

