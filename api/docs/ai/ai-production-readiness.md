# AI Platform Production Readiness v1

HelpDev AI hardening for reliability, observability, failure handling, governance, and E2E verification.
This document does **not** introduce new AI product features.

## Architecture

```
Admin /admin/ai
        │
        ▼
AiAdminController  (AdminOnly)
        │
        ├─ IAiAnalyticsQueries  → ai_usage_records (persisted)
        ├─ IAiOperationMetrics  → in-process counters
        └─ IAiHealthProbe       → config + connectivity only

Call sites (Content Assistant, Workflow Engine, RAG)
        │
        ▼
IAiTextGenerator (ResilientAiTextGenerator)
        │  retry (AiRetryPolicy) + metrics + AiGenerationResult
        ▼
FakeAiTextGenerator | HttpAiTextGenerator
```

| Concern | Owner |
|---------|--------|
| Generation adapters | `HelpDev.Infrastructure.Ai` |
| Contracts | `HelpDev.SharedContracts.Ai` |
| Usage persistence | Analytics `ai_usage_records` |
| Health | Observability `HealthCheckNames.Ai` |
| Content/RAG orchestration | Content / Search modules |

## Failure handling

- Call sites use `GenerateSafeAsync` → `AiGenerationResult`.
- Stable error codes: `ai_provider_unavailable`, `ai_generation_failed`, `ai_timeout`, `ai_invalid_response`.
- Provider failures must not crash workflows; RAG returns a safe user message and keeps sources.
- `AiRetryPolicy` retries **only** transient codes (timeout / provider unavailable / generation failed), with bounded exponential backoff (max 3 attempts by default).
- Unauthorized / validation / malformed responses are **not** retried.

## Security model

Never stored or logged:

- prompts / system instructions
- generated answer text
- API keys / bearer tokens

Usage records store: operation name, provider, model, token counts (if any), success, duration, error code, optional user/content ids.

Audit metadata stores operation name, outcome, and counts (e.g. `sourceCount`) — never prompts.

Health checks never issue a generation request (HEAD/config only for HTTP).

## Operational limits

- Max retry attempts: 3 (hard capped at 5 in policy construction).
- HTTP client timeout: 60s for generation; health connectivity: ~2s.
- AI health is non-critical by default (`Observability:Ai:IsCritical=false`).
- Disabled AI reports `Degraded` with `health_ai_disabled`.

## Governance (`AiPolicy`)

- Human approval required before publish
- No automatic publishing
- No secret transmission
- No private draft export outside controlled adapters
- AI output is suggestion only

Exposed at `GET /api/v1/admin/ai/policy` (Admin documentation).

## Known limitations

- In-process metrics reset on process restart; durable charts use `ai_usage_records`.
- HTTP health uses HEAD against the configured endpoint; some providers may not support HEAD (still treated as reachability when non-5xx).
- Fake provider is for local/test only — not production LLM quality.
- No autonomous agents, auto-publish, chatbot UI, or prompt storage.
