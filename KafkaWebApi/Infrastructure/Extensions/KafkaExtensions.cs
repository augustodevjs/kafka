using Confluent.Kafka;
using KafkaWebApi.Infrastructure.Kafka.Messaging;

namespace KafkaWebApi.Infrastructure.Extensions;

public static class KafkaExtensions
{
    public static void AddKafkaInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var producerConfig = new ProducerConfig();
        configuration.GetSection("Kafka:Producer").Bind(producerConfig);
        services.AddSingleton(new ProducerBuilder<string, string>(producerConfig).Build());
        
        services.AddScoped<IMessagePublisher, MessagePublisher>();
    }
}