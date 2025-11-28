using KafkaWebApi.Features.Orders;
using KafkaWebApi.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddWebApi();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddKafkaInfrastructure(builder.Configuration);

builder.Services.AddOrdersFeature();

var app = builder.Build();

app.UseSwaggerDocumentation();
app.MapControllers();

app.Run();
