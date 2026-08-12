using HelpDev.API.Security;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.RateLimiting;

namespace HelpDev.API.Security;

public sealed class AdminRateLimitConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        var routeTemplate = controller.Selectors
            .Select(selector => selector.AttributeRouteModel?.Template)
            .FirstOrDefault(template => !string.IsNullOrWhiteSpace(template));

        if (routeTemplate is null ||
            !routeTemplate.StartsWith("api/admin", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        foreach (var action in controller.Actions)
        {
            foreach (var selector in action.Selectors)
            {
                selector.EndpointMetadata.Add(new EnableRateLimitingAttribute(RateLimitPolicyNames.AdminMutation));
            }
        }
    }
}

public sealed class PublicContentRateLimitConvention : IControllerModelConvention
{
    public void Apply(ControllerModel controller)
    {
        if (!string.Equals(controller.ControllerName, "Content", StringComparison.Ordinal))
        {
            return;
        }

        foreach (var action in controller.Actions)
        {
            foreach (var selector in action.Selectors)
            {
                selector.EndpointMetadata.Add(new EnableRateLimitingAttribute(RateLimitPolicyNames.PublicContentRead));
            }
        }
    }
}
