namespace KafkaWebApi.Features.Orders;

public class Order
{
    public required string Id { get; init; }
    public required string CustomerId { get; init; }
    public required List<OrderItem> Items { get; init; }
}

public class OrderItem
{
    public required string ProductId { get; init; }
    public int Quantity { get; init; }
}

public record OrderResult(
    string OrderId,
    string Message,
    int ItemCount);

