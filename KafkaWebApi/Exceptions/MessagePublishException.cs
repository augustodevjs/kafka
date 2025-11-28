namespace KafkaWebApi.Exceptions;

public class MessagePublishException(string message, Exception innerException) : Exception(message, innerException);
