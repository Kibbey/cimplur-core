using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Domain.Exceptions;
using Memento.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

public static class CustomErrorHandler
{
    private static ILogger _logger;
    public static void UseCustomErrors(this IApplicationBuilder app, IHostEnvironment environment, ILogger logger)
    {
        _logger = logger;
        _logger.LogInformation("Hello World Logger");
        if (environment.IsDevelopment())
        {
            app.Use(WriteDevelopmentResponse);
        }
        else
        {
            app.Use(WriteProductionResponse);
        }
    }

    private static Task WriteDevelopmentResponse(HttpContext httpContext, Func<Task> next)
        => WriteResponse(httpContext, includeDetails: true);

    private static Task WriteProductionResponse(HttpContext httpContext, Func<Task> next)
        => WriteResponse(httpContext, includeDetails: false);

    private static async Task WriteResponse(HttpContext httpContext, bool includeDetails)
    {
        // Try and retrieve the error from the ExceptionHandler middleware
        var exceptionDetails = httpContext.Features.Get<IExceptionHandlerFeature>();
        var ex = exceptionDetails?.Error;

        // Should always exist, but best to be safe!
        if (ex != null)
        {
            _logger.LogInformation(ex.Message);
            // ProblemDetails has it's own content type
            httpContext.Response.ContentType = "application/problem+json";
            var problem = new ErrorResponse
            {
                Status = 500,
                Message = "An error occurred."
            };
            if (ex is BaseException) {
                var baseException = (BaseException)ex;
                problem = new ErrorResponse
                {
                    Status = baseException.Status,
                    Message = baseException.Message,
                    Data = baseException.PropertyErrors
                };
            } 
            

            // This is often very handy information for tracing the specific request
            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId != null)
            {
                problem.TraceId = traceId;
            }

            //Serialize the problem details object to the Response as JSON (using System.Text.Json)
            httpContext.Response.StatusCode = problem.Status;
            var result = JsonConvert.SerializeObject(problem);
            await httpContext.Response.WriteAsync(result);
        }
    }
}