using HelpDev.API.Controllers;
using HelpDev.Modules.Content.Application.SeoAnalysis.Dashboard;

namespace HelpDev.API.Tests;

public sealed class SeoAdminApiTests
{
    [Fact]
    public void Dashboard_endpoint_is_get_admin_seo_dashboard()
    {
        var method = typeof(SeoDashboardController).GetMethod(nameof(SeoDashboardController.GetDashboard));
        Assert.NotNull(method);
        var template = Assert.IsType<Microsoft.AspNetCore.Mvc.HttpGetAttribute>(
            method!.GetCustomAttributes(typeof(Microsoft.AspNetCore.Mvc.HttpGetAttribute), inherit: false).Single()).Template;
        Assert.Equal("dashboard", template);
    }

    [Fact]
    public void Seo_dashboard_dto_has_no_score_fields()
    {
        var names = typeof(SeoDashboardDto).GetProperties().Select(p => p.Name).ToArray();
        Assert.Contains("TotalContent", names);
        Assert.Contains("LastAnalysisTime", names);
        Assert.DoesNotContain(names, n => n.Contains("Score", StringComparison.OrdinalIgnoreCase));
    }
}
