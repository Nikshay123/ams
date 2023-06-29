using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace WebApp
{
    public class HttpHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public HttpHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(state =>
            {
                // Add Following
                context.Response.Headers.Add("X-Frame-Options", "DENY");
                context.Response.Headers.Add("X-XSS-Protection", "1; mode=block;");
                context.Response.Headers.Add("X-Content-Type-Options", "nosniff");
                context.Response.Headers.Add("Referrer-Policy", "origin");
                context.Response.Headers.Add("X-Permitted-Cross-Domain-Policies", "none");
                context.Response.Headers.Add("Content-Security-Policy", "default-src");
                context.Response.Headers.Add("Cache-Control", "no-cache; no-store; must-revalidate");
                context.Response.Headers.Add("Pragma", "no-cache");
                context.Response.Headers.Add("Expires", "0");
                // Not Sure about it
                //context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000; includeSubDomains; preload");
                context.Response.Headers.Add("Set-Cookie", "SameSite=None;Secure");

                return Task.FromResult(0);
            }, null);

            await _next(context);
        }
    }
}