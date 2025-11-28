using KafkaWebApi.Features.Orders.CreateOrder;
using KafkaWebApi.Features.Orders.ProcessOrder;

namespace KafkaWebApi.Features.Orders;

public static class OrdersModule
{
    public static void AddOrdersFeature(this IServiceCollection services)
    {
        services.AddScoped<ICreateOrderHandler, CreateOrderHandler>();
        services.AddScoped<IOrderProcessor, OrderProcessor>();
        
        services.AddHostedService<OrderConsumer>();
    }
}

