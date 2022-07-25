using BBBankAPI.Middleawares;

namespace BBBankAPI.Middlewares
{
    public static class CustomMiddlewaresExtensions
    {
        // Extention method exposing the middleware using IApplicationBuilder:
        public static IApplicationBuilder UseCustomLogginMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<LoggingMiddleware>();
        }

    }
}
