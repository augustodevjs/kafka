namespace KafkaWebApi.Infrastructure.Extensions;

public static class WebApiExtensions
{
    public static void AddWebApi(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddEndpointsApiExplorer();
    }
}