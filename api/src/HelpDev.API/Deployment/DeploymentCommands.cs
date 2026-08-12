using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using HelpDev.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Options;

namespace HelpDev.API.Deployment;

/// <summary>
/// Out-of-band operational commands invoked through the API executable. These commands never start
/// the HTTP server or hosted services (for example the Outbox processor) and never print secrets,
/// connection strings, or SQL. They exist to make deployment steps controlled and auditable.
/// </summary>
public enum DeploymentCommandKind
{
    /// <summary>Validate configuration and production safety without starting the server.</summary>
    Validate,

    /// <summary>Apply pending migrations under an advisory lock, then exit.</summary>
    ApplyMigrations,

    /// <summary>Emit the release manifest JSON artifact, then exit.</summary>
    EmitReleaseManifest,
}

public sealed record DeploymentCommand(
    DeploymentCommandKind Kind,
    bool ValidateDatabase = false,
    bool SeedRequiredSystemData = false,
    string? OutputPath = null,
    int? TestCount = null);

public static class DeploymentCommandParser
{
    public const string ValidateConfigFlag = "--validate-production-config";
    public const string ValidateDatabaseFlag = "--validate-database";
    public const string ApplyMigrationsFlag = "--apply-migrations";
    public const string SeedRequiredSystemDataFlag = "--seed-required-system-data";
    public const string SeedDevelopmentDemoFlag = "--seed-development-demo";
    public const string EmitReleaseManifestFlag = "--emit-release-manifest";
    public const string OutputFlag = "--output";
    public const string OutputShortFlag = "-o";
    public const string TestCountFlag = "--test-count";

    public static DeploymentCommand? Parse(string[] args)
    {
        if (args is null || args.Length == 0)
        {
            return null;
        }

        var hasValidateConfig = HasFlag(args, ValidateConfigFlag);
        var hasValidateDatabase = HasFlag(args, ValidateDatabaseFlag);
        var hasApplyMigrations = HasFlag(args, ApplyMigrationsFlag);
        var hasEmitManifest = HasFlag(args, EmitReleaseManifestFlag);

        if (hasEmitManifest)
        {
            return new DeploymentCommand(
                DeploymentCommandKind.EmitReleaseManifest,
                OutputPath: GetValue(args, OutputFlag) ?? GetValue(args, OutputShortFlag) ?? GetValue(args, EmitReleaseManifestFlag),
                TestCount: TryGetInt(args, TestCountFlag));
        }

        if (hasApplyMigrations)
        {
            return new DeploymentCommand(
                DeploymentCommandKind.ApplyMigrations,
                SeedRequiredSystemData: HasFlag(args, SeedRequiredSystemDataFlag));
        }

        if (hasValidateConfig || hasValidateDatabase)
        {
            return new DeploymentCommand(
                DeploymentCommandKind.Validate,
                ValidateDatabase: hasValidateDatabase);
        }

        return null;
    }

    public static bool HasDevelopmentDemoSeedRequest(string[] args) => HasFlag(args, SeedDevelopmentDemoFlag);

    private static bool HasFlag(string[] args, string flag) =>
        args.Any(a => string.Equals(a, flag, StringComparison.OrdinalIgnoreCase));

    private static string? GetValue(string[] args, string flag)
    {
        for (var i = 0; i < args.Length; i++)
        {
            if (!string.Equals(args[i], flag, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            if (i + 1 < args.Length && !args[i + 1].StartsWith("--", StringComparison.Ordinal) && args[i + 1] != "-o")
            {
                return args[i + 1];
            }

            return null;
        }

        return null;
    }

    private static int? TryGetInt(string[] args, string flag)
    {
        var value = GetValue(args, flag);
        return int.TryParse(value, out var parsed) ? parsed : null;
    }
}

public static class DeploymentCommands
{
    public const int ExitSuccess = 0;
    public const int ExitFailure = 1;

    /// <summary>
    /// Runs a host-backed deployment command (config validation or controlled migration). The provided
    /// application is already built but not started; the HTTP server and hosted services stay dormant.
    /// </summary>
    public static async Task<int> RunAsync(
        Microsoft.AspNetCore.Builder.WebApplication app,
        DeploymentCommand command,
        string[] rawArgs,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return command.Kind switch
            {
                DeploymentCommandKind.Validate => await RunValidateAsync(app, command, cancellationToken).ConfigureAwait(false),
                DeploymentCommandKind.ApplyMigrations => await RunApplyMigrationsAsync(app, command, rawArgs, cancellationToken).ConfigureAwait(false),
                _ => ExitFailure,
            };
        }
        catch (OptionsValidationException optionsValidationException)
        {
            // Options validators produce safe, secret-free failure messages.
            foreach (var failure in optionsValidationException.Failures)
            {
                Console.Error.WriteLine($"[config] ERROR: {failure}");
            }

            Console.Error.WriteLine($"[config] Production safety validation FAILED with {optionsValidationException.Failures.Count()} error(s).");
            return ExitFailure;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine($"[config] ERROR: {SafeMessage(ex)}");
            return ExitFailure;
        }
    }

    private static async Task<int> RunValidateAsync(
        Microsoft.AspNetCore.Builder.WebApplication app,
        DeploymentCommand command,
        CancellationToken cancellationToken)
    {
        var validator = app.Services.GetRequiredService<IProductionSafetyValidator>();
        var result = validator.Validate();

        foreach (var warning in result.Warnings)
        {
            Console.WriteLine($"[config] WARN: {warning}");
        }

        foreach (var error in result.Errors)
        {
            Console.Error.WriteLine($"[config] ERROR: {error}");
        }

        Console.WriteLine(result.IsValid
            ? "[config] Production safety validation passed."
            : $"[config] Production safety validation FAILED with {result.Errors.Count} error(s).");

        var success = result.IsValid;

        if (command.ValidateDatabase)
        {
            success &= await ValidateDatabaseAsync(app, cancellationToken).ConfigureAwait(false);
        }

        return success ? ExitSuccess : ExitFailure;
    }

    private static async Task<bool> ValidateDatabaseAsync(
        Microsoft.AspNetCore.Builder.WebApplication app,
        CancellationToken cancellationToken)
    {
        try
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var options = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseStartupOptions>>().Value;
            var migrationMode = options.ResolveMigrationMode(app.Environment.IsProduction());

            if (!context.Database.IsNpgsql())
            {
                Console.Error.WriteLine("[database] ERROR: configured provider is not PostgreSQL.");
                return false;
            }

            var canConnect = await context.Database.CanConnectAsync(cancellationToken).ConfigureAwait(false);
            if (!canConnect)
            {
                Console.Error.WriteLine("[database] ERROR: database is not reachable.");
                return false;
            }

            var applied = (await context.Database.GetAppliedMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToList();
            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken).ConfigureAwait(false)).ToList();

            Console.WriteLine(
                $"[database] provider=PostgreSQL reachable=true migrationMode={migrationMode} " +
                $"appliedMigrations={applied.Count} pendingMigrations={pending.Count} mutation=none");

            return true;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine($"[database] ERROR: {SafeMessage(ex)}");
            return false;
        }
    }

    private static async Task<int> RunApplyMigrationsAsync(
        Microsoft.AspNetCore.Builder.WebApplication app,
        DeploymentCommand command,
        string[] rawArgs,
        CancellationToken cancellationToken)
    {
        if (DeploymentCommandParser.HasDevelopmentDemoSeedRequest(rawArgs))
        {
            Console.Error.WriteLine("[migrate] ERROR: development/demo seeding is not permitted by the migration command.");
            return ExitFailure;
        }

        // Config safety is a precondition for applying migrations.
        var validator = app.Services.GetRequiredService<IProductionSafetyValidator>();
        var validation = validator.Validate();
        if (!validation.IsValid)
        {
            foreach (var error in validation.Errors)
            {
                Console.Error.WriteLine($"[migrate] ERROR: {error}");
            }

            Console.Error.WriteLine("[migrate] ERROR: refusing to apply migrations with an unsafe configuration.");
            return ExitFailure;
        }

        try
        {
            await DatabaseStartupManager.ApplyMigrationsAsync(
                app.Services,
                app.Environment,
                command.SeedRequiredSystemData,
                cancellationToken).ConfigureAwait(false);

            Console.WriteLine(command.SeedRequiredSystemData
                ? "[migrate] Migrations applied and required system data seeded."
                : "[migrate] Migrations applied.");
            return ExitSuccess;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            Console.Error.WriteLine($"[migrate] ERROR: {SafeMessage(ex)}");
            return ExitFailure;
        }
    }

    /// <summary>
    /// Emits the deterministic release manifest artifact. Reads release metadata only from the
    /// RELEASE_* environment variables and assembly attributes; never emits secrets, environment
    /// values, connection strings, or machine paths.
    /// </summary>
    public static async Task<int> EmitReleaseManifestAsync(DeploymentCommand command)
    {
        try
        {
            var apiAssembly = typeof(DeploymentCommands).Assembly;
            var infrastructureAssembly = typeof(ApplicationDbContext).Assembly;

            var version =
                NullIfEmpty(Environment.GetEnvironmentVariable("RELEASE_VERSION"))
                ?? apiAssembly.GetName().Version?.ToString(3)
                ?? "0.0.0";

            var commit = NullIfEmpty(Environment.GetEnvironmentVariable("RELEASE_COMMIT"));
            var buildTimestamp =
                NullIfEmpty(Environment.GetEnvironmentVariable("RELEASE_BUILD_TIMESTAMP"))
                ?? DateTimeOffset.UtcNow.ToString("O");

            var testCount = command.TestCount
                ?? TryParseInt(Environment.GetEnvironmentVariable("RELEASE_TEST_COUNT"));

            var manifest = new ReleaseManifest(
                Application: "HelpDev.API",
                Version: version,
                Commit: commit,
                BuildTimestampUtc: buildTimestamp,
                TargetFramework: "net8.0",
                Configuration: "Release",
                OpenApiVersion: "v1",
                MigrationCount: CountMigrations(infrastructureAssembly),
                TestCount: testCount,
                BinarySha256: ComputeAssemblySha256(apiAssembly));

            var outputPath = ResolveOutputPath(command.OutputPath);
            var directory = Path.GetDirectoryName(outputPath);
            if (!string.IsNullOrEmpty(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonSerializer.Serialize(manifest, ManifestJsonOptions);
            await File.WriteAllTextAsync(outputPath, json).ConfigureAwait(false);

            Console.WriteLine($"[manifest] Release manifest written to {outputPath}");
            return ExitSuccess;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"[manifest] ERROR: {SafeMessage(ex)}");
            return ExitFailure;
        }
    }

    private static readonly JsonSerializerOptions ManifestJsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
    };

    private static string ResolveOutputPath(string? outputPath)
    {
        var path = string.IsNullOrWhiteSpace(outputPath)
            ? Path.Combine("artifacts", "release", "release-manifest.json")
            : outputPath;

        return Path.IsPathRooted(path)
            ? path
            : Path.GetFullPath(Path.Combine(Environment.CurrentDirectory, path));
    }

    private static int CountMigrations(Assembly migrationsAssembly) =>
        migrationsAssembly
            .GetTypes()
            .Count(t => t.GetCustomAttributes(typeof(MigrationAttribute), inherit: false).Length > 0);

    private static string? ComputeAssemblySha256(Assembly assembly)
    {
        try
        {
            var location = assembly.Location;
            if (string.IsNullOrEmpty(location) || !File.Exists(location))
            {
                return null;
            }

            using var stream = File.OpenRead(location);
            var hash = SHA256.HashData(stream);
            return Convert.ToHexString(hash).ToLowerInvariant();
        }
        catch
        {
            return null;
        }
    }

    private static int? TryParseInt(string? value) =>
        int.TryParse(value, out var parsed) ? parsed : null;

    private static string? NullIfEmpty(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value;

    private static string SafeMessage(Exception ex) =>
        ex switch
        {
            InvalidOperationException => ex.Message,
            _ => ex.GetType().Name,
        };

    private sealed record ReleaseManifest(
        string Application,
        string Version,
        string? Commit,
        string BuildTimestampUtc,
        string TargetFramework,
        string Configuration,
        string OpenApiVersion,
        int MigrationCount,
        int? TestCount,
        string? BinarySha256);
}
