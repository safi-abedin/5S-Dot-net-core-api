namespace api.Helpers.Middleware
{
    public class LoggingEnrichmentMiddleware
    {
        private readonly RequestDelegate _next;

        public LoggingEnrichmentMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            var userId = context.User?.FindFirst("UserId")?.Value;
            var companyId = context.User?.FindFirst("CompanyId")?.Value;

            using (Serilog.Context.LogContext.PushProperty("UserId", userId ?? "0"))
            using (Serilog.Context.LogContext.PushProperty("CompanyId", companyId ?? "0"))
            using (Serilog.Context.LogContext.PushProperty("RequestPath", context.Request.Path))
            using (Serilog.Context.LogContext.PushProperty("ActionName", context.GetEndpoint()?.DisplayName))
            {
                await _next(context);
            }
        }
    }
}
