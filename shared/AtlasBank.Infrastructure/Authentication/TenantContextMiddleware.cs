using AtlasBank.Core.Application.Common;
using System.Security.Claims;

namespace AtlasBank.Infrastructure.Authentication;

/// <summary>
/// Middleware to extract tenant context from JWT claims
/// </summary>
public class TenantContextMiddleware
{
    private readonly RequestDelegate _next;

    public TenantContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("tenant_id");
            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier);
            var roleClaims = context.User.FindAll(ClaimTypes.Role);

            if (tenantContext is TenantContext tc)
            {
                tc.TenantId = tenantIdClaim?.Value ?? string.Empty;
                tc.UserId = userIdClaim?.Value ?? string.Empty;
                tc.Roles = roleClaims.Select(c => c.Value).ToList();
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension methods for adding tenant context to the pipeline
/// </summary>
public static class TenantContextExtensions
{
    public static IServiceCollection AddTenantContext(this IServiceCollection services)
    {
        services.AddScoped<ITenantContext, TenantContext>();
        return services;
    }

    public static IApplicationBuilder UseTenantContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantContextMiddleware>();
    }
}
