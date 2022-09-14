namespace BBBankAPI.Middlewares
{
    public class LoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<LoggingMiddleware> _logger;
        // Middleware implementation must include 
        public LoggingMiddleware(RequestDelegate next, ILogger<LoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                // this line will be executed when it enters the custom middleware and by that time it has the information about the method its trying to execute using the context.
                _logger.LogInformation("Entering " + context.Request.Path);
                // Call the next delegate/middleware in the pipeline which will be MVC and it will execute function that was called.
                await _next(context);
                // this line will be called after MVC is executed.
                _logger.LogInformation("Leaving " + context.Request.Path);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                throw;
            }
        }
    }
}
