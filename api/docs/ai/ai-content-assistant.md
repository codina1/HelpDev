# AI Content Assistant v1

Sprint 38 adds an on-demand editorial assistant for Admin CMS. Suggestions are
generated from **saved** content and never auto-applied, auto-saved, or published.

## Architecture

```
Admin CMS (Content Studio → AI Assistant tab)
        │  POST /api/v1/admin/content/{id}/ai/*
        ▼
ContentManagementController (WriterOrAdmin + ownership)
        ▼
Content.Application.ContentAi.IContentAiAssistantService
        │  loads content via IContentService (no DbContext)
        │  calls IAiTextGenerator (SharedContracts)
        │  records usage via IAiUsageRecorder
        │  audits request/failure (no prompt/body/output)
        ▼
HelpDev.Infrastructure.Ai  (Fake | Http providers)
        ▼
Analytics.ai_usage_records  (tokens/metadata only)
```

### Boundaries

| Layer | Allowed | Forbidden |
| --- | --- | --- |
| Content.Domain | — | Any AI types / SDKs |
| Content.Application | `IAiTextGenerator`, task enum, DTOs | Provider SDKs, HttpClient, OpenAI/Claude/Gemini packages |
| Infrastructure.Ai | Provider adapters, `AiProviderOptions` | Content DbContext usage |
| SharedContracts.Ai | `IAiTextGenerator`, usage recorder ports | Provider-specific types |

## Supported tasks (`ContentAiTaskType`)

| Task | API | Purpose |
| --- | --- | --- |
| ContentAnalysis | `POST .../ai/analyze` | Editorial structure/clarity notes (no scores) |
| TitleSuggestion | `POST .../ai/title-suggestions` | Title ideas |
| MetaDescription | `POST .../ai/meta-description` | SEO meta description draft |
| OutlineGeneration | `POST .../ai/outline` | H2/H3 outline |
| FaqGeneration | `POST .../ai/faq` | FAQ Q&A pairs |

v1 does **not** accept free-form user prompts.

## Response contract (`ContentAiResultDto`)

- `taskType`, `generatedText`, `createdAtUtc`, `model`, `provider`
- Never: API keys, system instructions, internal prompts, scores/ranks

## Configuration (`Ai` section)

| Key | Notes |
| --- | --- |
| `Enabled` | Master switch |
| `ProviderName` | `Fake` (tests/dev) or `Http` (generic JSON endpoint) |
| `Model` | Display / request model name |
| `Endpoint` | Required when `Http` + Enabled |
| `ApiKey` | Optional Bearer for Http — **never logged or returned** |
| `AllowedTasks` | Comma-separated task names |
| `DefaultMaxTokens` | 16–8192 |

Administration setting key constants (for later CMS wiring, not secrets):

- `Ai.Enabled`
- `Ai.DefaultModel`
- `Ai.AllowedTasks`

Runtime feature gate in v1 reads `AiProviderOptions` (appsettings / env).

## PromptLab

PromptLab remains the template/render engine (variables, versions). Content AI v1
uses fixed controlled instructions in Application. Future work can bind
PromptLab templates for system instructions without moving provider SDKs into
Content.

## Security & audit

| Action | Metadata keys |
| --- | --- |
| `content.ai_task_requested` | `taskType`, `contentId` |
| `content.ai_task_failed` | `taskType`, `contentId`, `failureCode` |

Never audit/log: prompts, generated text, content body, API keys.

Authorization: `WriterOrAdmin`. Writers only for owned content (same
`content_not_found` masking as other management APIs).

## Usage tracking

Table `ai_usage_records` (Analytics module):

`Id`, `UserId`, `TaskType`, `Provider`, `Model`, `InputTokens`, `OutputTokens`,
`ContentId?`, `CreatedAtUtc`

No prompt/body/output columns.

## Frontend

Route: `/admin/content/[id]/ai`

Components under `components/admin/content/ai/`:

- `content-ai-panel.tsx` — actions
- `ai-action-card.tsx`, `ai-result-viewer.tsx`, `ai-loading-state.tsx`

Flow: click → API → show result → human copies/applies manually.

## Limitations (v1)

- No chat UI / ChatGPT clone
- No autonomous publishing or rewriting
- No automatic content replacement
- No AI ranking/scores
- No external crawling
- Fake provider is for tests/dev only (`[Fake]` labeled output)
- Http provider is a generic JSON adapter — not an OpenAI SDK wrapper
- Generated text is not persisted as content revisions

## Future extensions

- PromptLab-backed configurable instructions
- Admin UI for `Ai.Enabled` / model / allowed tasks (no secret display)
- Usage dashboards from `ai_usage_records`
- Additional controlled task types (still no free-form prompts without review)
