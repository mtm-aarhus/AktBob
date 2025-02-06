using Hangfire.Dashboard;

namespace AktBob.Api;

internal class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();

        // Allow all authenticated users to see the Dashboard (potentially dangerous).
        return httpContext.User.Identity?.IsAuthenticated ?? false;
    }
}
