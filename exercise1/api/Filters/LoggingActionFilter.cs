using Microsoft.AspNetCore.Mvc.Filters;
using StargateAPI.Business.Services;
using System.Text.Json;

namespace StargateAPI.Filters
{
    public class LoggingActionFilter : IAsyncActionFilter
    {
        private readonly IProcessLoggingService _loggingService;

        public LoggingActionFilter(IProcessLoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var controller = context.Controller.GetType().Name.Replace("Controller", "");
            var action = context.ActionDescriptor.RouteValues["action"] ?? "Unknown";
            
            // Get request data for logging
            string? requestData = null;
            if (context.ActionArguments.Any())
            {
                try
                {
                    requestData = JsonSerializer.Serialize(context.ActionArguments);
                }
                catch
                {
                    requestData = "Unable to serialize request data";
                }
            }

            // Log start of action
            await _loggingService.LogInfoAsync(
                $"Starting {controller}.{action}", 
                controller, 
                action, 
                requestData
            );

            var result = await next();

            if (result.Exception == null)
            {
                // Log success
                await _loggingService.LogSuccessAsync(
                    $"Successfully completed {controller}.{action}", 
                    controller, 
                    action
                );
            }
            else
            {
                // Log error
                await _loggingService.LogErrorAsync(
                    $"Failed to complete {controller}.{action}", 
                    result.Exception, 
                    controller, 
                    action, 
                    requestData
                );
            }
        }
    }
}