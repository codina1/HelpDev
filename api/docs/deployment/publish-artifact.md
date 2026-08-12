# Publish Artifact

Deterministic publish and verification process for the HelpDev backend. Part of
**Sprint 26 — Production Deployment Validation & Go-Live Readiness**.

The goal is a reproducible, self-contained deployment artifact whose contents and hashes
can be recorded as release evidence. See
[../release/release-evidence-template.md](../release/release-evidence-template.md).

## Publish command

Build a Release publish output:

```bash
dotnet publish src/HelpDev.API/HelpDev.API.csproj -c Release -o artifacts/publish/helpdev-api
```

Publish from a clean tree at the intended commit, with `0 warnings / 0 errors`. Record the
commit and version used.

## The artifact MUST contain

- **Application binaries** — `HelpDev.API.dll` and all runtime dependency assemblies.
- **Runtime configuration files** — `appsettings.json` and **`appsettings.Production.json`**
  (non-secret policy defaults only; no secrets are committed).
- **XML documentation files** — required only if OpenAPI is intentionally enabled to enrich
  the generated spec. OpenAPI is disabled in Production by default.
- **Runtime config metadata** — `HelpDev.API.deps.json` and `HelpDev.API.runtimeconfig.json`.

## The artifact MUST NOT contain

- **Test assemblies** or test projects (e.g. `*.Tests.dll`).
- **Source files** (`*.cs`, project files) beyond what publish normally emits.
- **User secrets** or any file with real credentials, connection strings, or keys.
- **Development database** files, dumps, or fixtures.
- **Testcontainers** files or container definitions used only for testing.
- **Integration/test configuration** (e.g. `appsettings.Development.json`,
  `appsettings.Testing.json`, integration test settings).
- **Exported logs**, `.log` files, or captured request/response dumps.
- **Temporary files** (`bin/`, `obj/` intermediates, editor/OS temp files).

## Generate and verify hashes

Compute a hash for the primary binary (and optionally the whole artifact) and record it in
the release evidence.

```bash
# Linux/macOS
sha256sum artifacts/publish/helpdev-api/HelpDev.API.dll
```

```powershell
# Windows PowerShell
Get-FileHash artifacts\publish\helpdev-api\HelpDev.API.dll -Algorithm SHA256
```

The release manifest also records the primary binary hash as `binarySha256`, computed by
the emit command below — cross-check the two.

## Generate the release manifest

The API executable emits a **deterministic** manifest without starting the server or hosted
services. It reads only `RELEASE_*` environment variables and assembly attributes and never
emits secrets, environment values, connection strings, or machine paths.

```bash
dotnet HelpDev.API.dll --emit-release-manifest -o artifacts/release/release-manifest.json --test-count <n>
```

Example manifest fields:

```json
{
  "application": "HelpDev.API",
  "version": "<version>",
  "commit": "<commit>",
  "buildTimestampUtc": "<utc-timestamp>",
  "targetFramework": "net8.0",
  "configuration": "Release",
  "openApiVersion": "v1",
  "migrationCount": 11,
  "testCount": <n>,
  "binarySha256": "<hex>"
}
```

- `version` falls back to the assembly version when `RELEASE_VERSION` is unset.
- `commit` and `buildTimestampUtc` come from `RELEASE_COMMIT` / `RELEASE_BUILD_TIMESTAMP`
  (timestamp falls back to now if unset).
- `migrationCount` is derived from the migrations assembly and should be **11** for v1.
- `testCount` comes from `--test-count` or `RELEASE_TEST_COUNT`.
- `binarySha256` is the SHA-256 of the published `HelpDev.API.dll` (may be null if the
  binary is unavailable).

## Verification checklist

- [ ] Published with `dotnet publish -c Release` from a clean tree at the target commit.
- [ ] Build produced `0 warnings / 0 errors`.
- [ ] Artifact contains application binaries, `appsettings.json`, and
      `appsettings.Production.json`.
- [ ] Artifact contains no test assemblies, source files, user secrets, dev database,
      Testcontainers files, integration/test config, exported logs, or temp files.
- [ ] `appsettings.Production.json` contains no secrets (placeholders/defaults only).
- [ ] SHA-256 of `HelpDev.API.dll` computed and recorded.
- [ ] Release manifest emitted; `migrationCount = 11`; `binarySha256` matches the computed
      hash.
- [ ] `version`, `commit`, and `buildTimestampUtc` in the manifest match the intended
      release.
- [ ] `--validate-production-config` passes against the target configuration.
- [ ] Previous artifact retained for rollback.

## Related

- [production-config-example.md](production-config-example.md)
- [../release/release-evidence-template.md](../release/release-evidence-template.md)
- [../release/go-live-checklist.md](../release/go-live-checklist.md)
- [release-runbook.md](release-runbook.md)
- [rollback-runbook.md](rollback-runbook.md)
