using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace ClinicManagementSystem_Final.Middleware
{
    public class NoCacheAuthenticatedMiddleware
    {
        private readonly RequestDelegate _next;

        public NoCacheAuthenticatedMiddleware(RequestDelegate next) => _next = next;

        private void TrySetNoCacheHeaders(HttpContext context)
        {
            if (context == null) return;
            try
            {
                if (context.Response.HasStarted) return;
                context.Response.Headers["Cache-Control"] = "no-cache, no-store, must-revalidate";
                context.Response.Headers["Pragma"] = "no-cache";
                context.Response.Headers["Expires"] = "0";
            }
            catch
            {
                // swallowing any header-set errors to avoid breaking the request
            }
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // If user is authenticated in this session, mark the response as no-cache.
            // Set headers before the rest of the pipeline if possible (safe), and again after.
            if (context.Session != null && context.Session.GetInt32("StaffId") != null)
            {
                TrySetNoCacheHeaders(context);
            }

            await _next(context);

            // Attempt again after pipeline in case session was set during the request
            if (context.Session != null && context.Session.GetInt32("StaffId") != null)
            {
                TrySetNoCacheHeaders(context);
            }
        }
    }
}