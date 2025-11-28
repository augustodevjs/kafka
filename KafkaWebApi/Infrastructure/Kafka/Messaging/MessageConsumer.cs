using System.Text.Json;
using Confluent.Kafka;

namespace KafkaWebApi.Infrastructure.Kafka.Messaging;

public abstract class MessageConsumer<T>(
    IConfiguration configuration, 
    ILogger logger) : BackgroundService
    where T : class
{
    private IConsumer<string, string>? _consumer;

    protected abstract string Topic { get; }
    protected abstract string ConsumerGroupId { get; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig();
        configuration.GetSection("Kafka:Consumer").Bind(config);
        config.GroupId = ConsumerGroupId;

        _consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => logger.LogError("Message broker error | Topic: {Topic} | Reason: {Reason}", Topic, e.Reason))
            .Build();

        _consumer.Subscribe(Topic);
        logger.LogInformation("Consumer started | Topic: {Topic} | GroupId: {GroupId}", Topic, ConsumerGroupId);

        await Task.Yield();

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var result = _consumer.Consume(stoppingToken);

                    if (result?.Message != null)
                    {
                        await ProcessMessageAsync(result, config.EnableAutoCommit ?? true, stoppingToken);
                    }
                }
                catch (ConsumeException ex)
                {
                    logger.LogError(ex, "Error consuming message | Topic: {Topic} | Reason: {Reason}", Topic, ex.Error.Reason);
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            logger.LogInformation("Consumer stopped | Topic: {Topic}", Topic);
        }
    }

    private async Task ProcessMessageAsync(ConsumeResult<string, string> result, bool autoCommitEnabled, CancellationToken cancellationToken)
    {
        T? message = null;
        var shouldCommit = false;
        
        try
        {
            message = JsonSerializer.Deserialize<T>(result.Message.Value, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true
            });

            if (message != null)
            {
                await HandleMessageAsync(message, result.Message.Key, cancellationToken);
                shouldCommit = true;
                
                logger.LogInformation("Message processed successfully | Topic: {Topic} | Key: {Key} | Partition: {Partition} | Offset: {Offset}", 
                    Topic, result.Message.Key, result.Partition.Value, result.Offset.Value);
            }
            else
            {
                logger.LogWarning("Deserialized message is null | Topic: {Topic} | Key: {Key} | Offset: {Offset}", 
                    Topic, result.Message.Key, result.Offset.Value);
                shouldCommit = true; 
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "Failed to deserialize message | Topic: {Topic} | Key: {Key} | Offset: {Offset}", 
                Topic, result.Message.Key, result.Offset.Value);
            
            shouldCommit = true;
            
            logger.LogWarning("Skipping message due to deserialization error | Topic: {Topic} | Key: {Key}", 
                Topic, result.Message.Key);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to process message | Topic: {Topic} | Key: {Key} | Offset: {Offset}", 
                Topic, result.Message.Key, result.Offset.Value);
            
            shouldCommit = false;
        }
        finally
        {
            if (shouldCommit && !autoCommitEnabled)
            {
                try
                {
                    _consumer!.Commit(result);
                    logger.LogDebug("Offset committed | Topic: {Topic} | Partition: {Partition} | Offset: {Offset}", 
                        Topic, result.Partition.Value, result.Offset.Value);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to commit offset | Topic: {Topic} | Partition: {Partition} | Offset: {Offset}", 
                        Topic, result.Partition.Value, result.Offset.Value);
                }
            }
        }
    }

    protected abstract Task HandleMessageAsync(T message, string key, CancellationToken cancellationToken);

    public override void Dispose()
    {
        try
        {
            _consumer?.Close();
            _consumer?.Dispose();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error disposing consumer | Topic: {Topic}", Topic);
        }
        finally
        {
            base.Dispose();
        }
    }
}

