using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Confluent.Kafka;
using KafkaWebApi.Exceptions;

namespace KafkaWebApi.Infrastructure.Kafka.Messaging;

public interface IMessagePublisher
{
    Task<DeliveryResult<string, string>> PublishAsync<T>(
        string topic, 
        string key, 
        T message, 
        Headers? headers = null,
        CancellationToken cancellationToken = default) where T : class;
}

public class MessagePublisher(
    IProducer<string, string> producer,
    ILogger<MessagePublisher> logger) : IMessagePublisher
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public async Task<DeliveryResult<string, string>> PublishAsync<T>(
        string topic, 
        string key, 
        T message, 
        Headers? headers = null,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(topic);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentNullException.ThrowIfNull(message);

        var messageType = typeof(T).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogDebug("Publishing message | Type: {MessageType} | Topic: {Topic} | Key: {Key}", messageType, topic, key);
        
        try
        {
            var kafkaMessage = BuildKafkaMessage(message, key, messageType, headers);
            var result = await producer.ProduceAsync(topic, kafkaMessage, cancellationToken);
            
            stopwatch.Stop();
            
            logger.LogInformation("Message published | Type: {MessageType} | Topic: {Topic} | Key: {Key} | Partition: {Partition} | Offset: {Offset} | Latency: {Latency}ms",
                messageType, topic, key, result.Partition.Value, result.Offset.Value, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (ProduceException<string, string> ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Kafka publish failed | Type: {MessageType} | Topic: {Topic} | Key: {Key} | ErrorCode: {ErrorCode} | Reason: {Reason} | Latency: {Latency}ms",
                messageType, topic, key, ex.Error.Code, ex.Error.Reason, stopwatch.ElapsedMilliseconds);
            throw new MessagePublishException($"Failed to publish message. Error: {ex.Error.Reason}", ex);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            logger.LogError(ex, "Unexpected publish error | Type: {MessageType} | Topic: {Topic} | Key: {Key} | Latency: {Latency}ms",
                messageType, topic, key, stopwatch.ElapsedMilliseconds);
            throw;
        }
    }

    private Message<string, string> BuildKafkaMessage<T>(T message, string key, string messageType, Headers? headers) where T : class
    {
        var json = JsonSerializer.Serialize(message, _jsonOptions);
        
        var messageHeaders = headers ?? [];
        messageHeaders.Add("message-type", Encoding.UTF8.GetBytes(messageType));
        messageHeaders.Add("published-at", Encoding.UTF8.GetBytes(DateTime.UtcNow.ToString("o")));
        messageHeaders.Add("correlation-id", Encoding.UTF8.GetBytes(Guid.NewGuid().ToString()));
        
        var kafkaMessage = new Message<string, string>
        {
            Key = key,
            Value = json,
            Headers = messageHeaders
        };

         return kafkaMessage;
    }
}

