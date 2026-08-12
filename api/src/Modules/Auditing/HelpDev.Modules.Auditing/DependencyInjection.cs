using HelpDev.Modules.Auditing.Application.Persistence;
using HelpDev.Modules.Auditing.Application.Queries;
using HelpDev.Modules.Auditing.Application.Recording;
using HelpDev.Modules.Auditing.Domain;
using HelpDev.Modules.Auditing.Infrastructure.Persistence;
using HelpDev.SharedContracts.Auditing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace HelpDev.Modules.Auditing;

public static class DependencyInjection
{
    public static IServiceCollection AddAuditingModule(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddSingleton<IValidateOptions<AuditOptions>, AuditOptionsValidator>();
        services.AddOptions<AuditOptions>()
            .Bind(configuration.GetSection(AuditOptions.SectionName))
            .ValidateOnStart();

        services.AddSingleton<IAuditPersistenceFailureInjector, NoOpAuditPersistenceFailureInjector>();
        services.AddScoped<IAuditRecordRepository, AuditRecordRepository>();
        services.AddScoped<IAuditMetadataSanitizer, AuditMetadataSanitizer>();
        services.AddScoped<IAuditRecorder, AuditRecorder>();
        services.AddScoped<IAuditQueries, AuditQueries>();

        return services;
    }
}

public sealed class AuditOptionsValidator : IValidateOptions<AuditOptions>
{
    public ValidateOptionsResult Validate(string? name, AuditOptions options)
    {
        if (options.RetentionDays <= 0)
        {
            return ValidateOptionsResult.Fail("Audit retention days must be positive.");
        }

        if (options.MaxMetadataEntries <= 0)
        {
            return ValidateOptionsResult.Fail("Audit max metadata entries must be positive.");
        }

        if (options.MaxMetadataKeyLength <= 0 || options.MaxMetadataValueLength <= 0)
        {
            return ValidateOptionsResult.Fail("Audit metadata length limits must be positive.");
        }

        if (options.MaxReasonLength <= 0)
        {
            return ValidateOptionsResult.Fail("Audit max reason length must be positive.");
        }

        return ValidateOptionsResult.Success;
    }
}
