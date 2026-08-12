# Tool Library Platform v1

**Sprint:** 48  
**Scope:** Content Core + Tool Metadata + Features + Alternatives (modular monolith).

## Architecture

```
Content (lifecycle / SEO / search / revision / workflow)
  └── ToolMetadata (1:1 satellite)
        ├── ToolFeature (1:N)
        └── ToolAlternative (1:N → other ContentId)
```

- Content owns publish, SEO, media, revisions, workflow, and search indexing.
- Tool satellite stores **only** catalog fields (name, URLs, pricing, platforms, license, features, alternatives).
- No tool columns on `contents`, no JSON mega-metadata, no Toolbox coupling.
- Search continues to index Content events only.

## Domain

| Entity | Namespace | Notes |
|--------|-----------|-------|
| `ToolMetadata` | `Domain.Tools` | 1:1 with Content (`ContentType.Tool`) |
| `ToolFeature` | `Domain.Tools` | Ordered title/description rows |
| `ToolAlternative` | `Domain.Tools` | Points at another tool `ContentId` |

Enums: `PricingModel`, `PlatformSupport` (flags), `LicenseType`.

## Persistence

Migration: `AddToolLibraryV1` (count: **23**)

| Table | Indexes / FK |
|-------|----------------|
| `tool_metadata` | unique `content_id`, index `tool_name`, FK → `contents` CASCADE |
| `tool_features` | FK → `tool_metadata` CASCADE |
| `tool_alternatives` | FK → `tool_metadata` CASCADE |

## Application

- `IToolService` / `ToolService` — Create, Update, GetByContentId, AddFeature, RemoveFeature
- `IToolQueries` / `ToolQueries` — List (ownership-scoped)
- DTOs: `ToolDetailDto`, `ToolFeatureDto`, `ToolListItemDto` (+ alternatives DTO)
- AI foundation: `IToolAiAssistantService` — summary / feature suggestions, **human apply only**

## API (WriterOrAdmin + ownership)

| Method | Route |
|--------|-------|
| GET | `/api/v1/admin/tools` |
| GET | `/api/v1/admin/content/{id}/tool` |
| PUT | `/api/v1/admin/content/{id}/tool` (upsert) |
| POST | `/api/v1/admin/content/{id}/tool/features` |
| DELETE | `/api/v1/admin/content/{id}/tool/features/{featureId}` |
| POST | `/api/v1/admin/content/{id}/tool/ai/summary` |
| POST | `/api/v1/admin/content/{id}/tool/ai/features` |

Writer → own content only. Admin → all. Wrong owner masked as `content_not_found`.

## Frontend

- `/admin/content/tools` — list (`ContentType=Tool`)
- `/admin/content/tools/new` — create Content + upsert tool metadata
- `/admin/content/tools/[id]` — ToolWorkspace (catalog, features, alternatives, AI suggestions, preview)
- Studio/SEO via existing `/admin/content/{id}/edit`
- Public foundation: `/tools/[slug]` (placeholder; no full public page)

## SEO

- Reuses existing SEO system on Content.
- `IStructuredDataGenerator.GenerateSoftwareApplication` foundation — **not** injected on public pages yet.

## Limitations

- Public `/tools/[slug]` is a route foundation only
- SoftwareApplication schema not emitted publicly yet
- AI suggestions never auto-save
- Alternatives referenced by ContentId (no picker UI beyond GUID entry)
- Distinct from Toolbox `ToolDefinition` catalog/execution APIs
