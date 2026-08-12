using Microsoft.OpenApi.Any;

namespace HelpDev.API.OpenApi;

public static class OpenApiExampleCatalog
{
    public static IOpenApiAny? GetRequestExample(string operationId) =>
        operationId switch
        {
            "Auth_SendOtp" or "Auth_RequestOtp" => new OpenApiObject
            {
                ["mobile"] = new OpenApiString("09120000000"),
            },
            "Auth_VerifyOtp" => new OpenApiObject
            {
                ["mobile"] = new OpenApiString("09120000000"),
                ["code"] = new OpenApiString("123456"),
            },
            "ToolboxCatalog_Execute" => new OpenApiObject
            {
                ["input"] = new OpenApiObject
                {
                    ["text"] = new OpenApiString("hello"),
                },
            },
            "PromptLabCatalog_Render" => new OpenApiObject
            {
                ["variables"] = new OpenApiObject
                {
                    ["topic"] = new OpenApiString("productivity"),
                },
            },
            "Profile_UpdateMyProfile" => new OpenApiObject
            {
                ["displayName"] = new OpenApiString("Sample User"),
            },
            _ => null,
        };

    public static IOpenApiAny? GetResponseExample(string operationId) =>
        operationId switch
        {
            "Auth_VerifyOtp" => new OpenApiObject
            {
                ["accessToken"] = new OpenApiString("<jwt-token>"),
                ["expiresAtUtc"] = new OpenApiString("2026-07-20T13:30:00Z"),
            },
            "Auth_SendOtp" or "Auth_RequestOtp" => new OpenApiObject
            {
                ["message"] = new OpenApiString("OTP sent successfully."),
            },
            "Operations_GetLiveness" or "Operations_GetReadiness" => new OpenApiObject
            {
                ["status"] = new OpenApiString("Healthy"),
            },
            "Profile_GetMyProfile" => new OpenApiObject
            {
                ["id"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                ["mobile"] = new OpenApiString("09120000000"),
                ["displayName"] = new OpenApiString("Sample User"),
            },
            "Content_ListPublished" => new OpenApiArray
            {
                new OpenApiObject
                {
                    ["id"] = new OpenApiString("22222222-2222-2222-2222-222222222222"),
                    ["slug"] = new OpenApiString("getting-started"),
                    ["title"] = new OpenApiString("Getting Started"),
                },
            },
            "Search_Search" => new OpenApiObject
            {
                ["query"] = new OpenApiString("help"),
                ["page"] = new OpenApiInteger(1),
                ["pageSize"] = new OpenApiInteger(20),
                ["total"] = new OpenApiInteger(0),
                ["items"] = new OpenApiArray(),
            },
            "Health" => new OpenApiObject
            {
                ["status"] = new OpenApiString("Healthy"),
            },
            _ => null,
        };
}
