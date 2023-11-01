using System.Net;
using System.Text.Json;

namespace friasco_api.Helpers;

public class ErrorHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ErrorHandlerMiddleware> _logger;

    public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext httpContext)
    {
        try
        {
            await _next(httpContext);
        }
        catch (Exception error)
        {
            var response = httpContext.Response;
            response.ContentType = "application/json";

            var errorDetails = new ErrorResponse()
            {
                Title = "One or more errors occurred."
            };

            switch (error)
            {
                case AppException e:
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    errorDetails.Status = (int)HttpStatusCode.BadRequest;
                    errorDetails.Errors.Add(nameof(Exception), new[] { e.Message });
                    break;
                case KeyNotFoundException e:
                    response.StatusCode = (int)HttpStatusCode.NotFound;
                    errorDetails.Status = (int)HttpStatusCode.NotFound;
                    errorDetails.Errors.Add(nameof(Exception), new[] { e.Message });
                    break;
                default:
                    _logger.LogError(error, error.Message);
                    response.StatusCode = (int)HttpStatusCode.InternalServerError;
                    errorDetails.Status = (int)HttpStatusCode.InternalServerError;
                    errorDetails.Errors.Add(nameof(Exception), new[] { error.Message });
                    break;
            }

            var result = JsonSerializer.Serialize(errorDetails);
            await response.WriteAsync(result);
        }
    }
}
