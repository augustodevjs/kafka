using KafkaWebApi.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace KafkaWebApi.Features.Orders.CreateOrder;

[ApiController]
[Route("api/orders")]
public class CreateOrderEndpoint(
    ICreateOrderHandler handler, 
    ILogger<CreateOrderEndpoint> logger) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(OrderResult), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateOrder(
        [FromBody] CreateOrderCommand command, 
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await handler.HandleAsync(command, cancellationToken);
            return Ok(result);
        }
        catch (MessagePublishException ex)
        {
            logger.LogError(ex, "Failed to publish order | OrderId: {OrderId}", command.Id);
            
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                error = "Message broker unavailable",
                message = "Unable to process order at this time. Please try again later.",
                orderId = command.Id
            });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Unexpected error creating order | OrderId: {OrderId}", command.Id);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new
            {
                error = "Internal server error",
                message = "An unexpected error occurred while processing your order.",
                orderId = command.Id
            });
        }
    }
}

