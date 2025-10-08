using StargateAPI.Business.Data;

namespace StargateAPI.Business.Services
{
    public interface IProcessLoggingService
    {
        Task LogSuccessAsync(string message, string? controller = null, string? action = null, string? requestData = null);
        Task LogErrorAsync(string message, Exception? exception = null, string? controller = null, string? action = null, string? requestData = null);
        Task LogInfoAsync(string message, string? controller = null, string? action = null, string? requestData = null);
        Task LogWarningAsync(string message, string? controller = null, string? action = null, string? requestData = null);
    }

    public class ProcessLoggingService : IProcessLoggingService
    {
        private readonly StargateContext _context;

        public ProcessLoggingService(StargateContext context)
        {
            _context = context;
        }

        public async Task LogSuccessAsync(string message, string? controller = null, string? action = null, string? requestData = null)
        {
            await LogAsync("SUCCESS", message, null, controller, action, requestData);
        }

        public async Task LogErrorAsync(string message, Exception? exception = null, string? controller = null, string? action = null, string? requestData = null)
        {
            await LogAsync("ERROR", message, exception?.ToString(), controller, action, requestData);
        }

        public async Task LogInfoAsync(string message, string? controller = null, string? action = null, string? requestData = null)
        {
            await LogAsync("INFO", message, null, controller, action, requestData);
        }

        public async Task LogWarningAsync(string message, string? controller = null, string? action = null, string? requestData = null)
        {
            await LogAsync("WARNING", message, null, controller, action, requestData);
        }

        private async Task LogAsync(string level, string message, string? exception = null, string? controller = null, string? action = null, string? requestData = null)
        {
            try
            {
                var log = new ProcessLog
                {
                    Timestamp = DateTime.UtcNow,
                    Level = level,
                    Message = message,
                    Exception = exception,
                    Controller = controller,
                    Action = action,
                    RequestData = requestData
                };

                _context.ProcessLogs.Add(log);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to log to database: {ex.Message}");
            }
        }
    }
}