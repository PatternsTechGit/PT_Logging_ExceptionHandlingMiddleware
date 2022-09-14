using BBBankAPI.Middlewares;

namespace BBBankAPI
{
    public static class CustomMiddlewaresExtensions
    {
        // Extension method exposing the middleware using IApplicationBuilder:
        public static IApplicationBuilder UseCustomLogginMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }

    }
}
