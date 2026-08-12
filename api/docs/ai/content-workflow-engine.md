# AI Content Workflow Engine v1

Human-controlled AI-assisted content production for HelpDev Admin CMS.

## Human approval model

```
Idea → AI Research (RAG) → Outline → Draft → SEO suggestions → Review → Human Publish
```

AI **never** auto-saves generated text into Content, never auto-publishes, and never bypasses:

`Draft → ReviewPending → Approved → Published`

## Architecture

```
Admin UI  /admin/content/workflows
        │
        ▼
ContentWorkflowEngineController  (WriterOrAdmin + ownership)
        │
        ▼
Content.Application.AiWorkflow
  ├─ ContentIdea / AiContentWorkflowSession (persisted metadata only)
  ├─ IWorkflowKnowledgeRetriever  → Infrastructure adapter → Search IRagContextBuilder
  ├─ IAiTextGenerator             → Infrastructure.Ai
  ├─ IContentSeoAnalyzer          → deterministic SEO
  └─ IContentService + Revisions  → apply draft only on explicit user action
```

### Boundaries

| Module | Owns |
|--------|------|
| Content | Ideas, sessions, apply-draft → Content + Revision |
| Search | Knowledge retrieval (via port/adapter) |
| AI Infrastructure | Providers (`Fake` / `Http`) |
| Analytics | `ai_usage_records` (tokens/metadata only) |

Generated research/outline/draft text is **returned to the client** and not stored in workflow tables unless the user clicks **Apply draft**.

## Workflow states

### Idea (`ContentIdeaStatus`)

Draft → Researching → Writing → Review → Completed | Cancelled

Transitions are explicit domain commands only.

### Session steps (`AiContentWorkflowStep`)

Research → Outline → Draft → Seo → Review

## APIs (`WriterOrAdmin`)

| Method | Path |
|--------|------|
| POST | `/api/v1/admin/content/workflows` |
| GET | `/api/v1/admin/content/workflows` |
| GET | `/api/v1/admin/content/workflows/{id}` |
| POST | `.../research` |
| POST | `.../outline` |
| POST | `.../draft` |
| POST | `.../seo` |
| POST | `.../apply-draft` |

Writers manage own workflows; Admins manage all.

## Security

- No AI output mutates the database without `apply-draft`
- No prompts, private drafts, or generated text in `ai_usage_records`
- No provider keys in API responses
- Ownership uses `ContentManagementActor` (writers get not-found for others)

## Database

Migration `AddAiContentWorkflowEngineV1`:

- `content_ideas`
- `ai_content_workflow_sessions`

`ExpectedMigrationCount = 19`

## Future roadmap

- Persist optional step snapshots with explicit user opt-in
- PromptLab template binding for workflow instructions
- Multi-language research filters
- Admin bulk cancel for stale ideas
