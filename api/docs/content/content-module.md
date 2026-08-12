# Content Module

`HelpDev.Modules.Content` owns authored website content (news, articles, roadmap steps,
tools, prompts, courses references). It began as a public content provider and is now a
CMS-ready content management module: it supports admin listing, editing, publishing and
CMS metadata while keeping the existing public API contracts unchanged.

## Layers

- **Domain** – `Content` aggregate, `Slug` value object, `ContentType`/`ContentStatus`
  enums, and domain events. No EF Core, no ASP.NET.
- **Application** – services, ports (`IContentRepository`, `IContentDbContext`), read
  models (`IAdminContentQueries`), DTOs and the `ContentException` error model. Depends only
  on abstractions (`IUnitOfWork`, `IDateTimeProvider`), never on Infrastructure.
- **Infrastructure** – `ContentRepository`, `AdminContentQueries`, EF configuration. The
  `IContentDbContext` port is implemented by the shared `ApplicationDbContext`.

## Article & News metadata extensions (Sprint 47B)

Satellite 1:1 tables `article_metadata` / `news_metadata` hang off `Content` via
`ContentId` (unique + FK). Domain lives under `Domain.Articles` / `Domain.News`.
Services: `IArticleMetadataService`, `INewsMetadataService`. Search schema is unchanged;
SEO continues to use the existing owned `SeoMetadata` / `PUT .../seo` path.

See also: `docs/admin/article-news-cms-v1.md`.

## Tool Library extension (Sprint 48)

Satellite tables `tool_metadata` (+ `tool_features`, `tool_alternatives`) hang off
`Content` when `ContentType=Tool`. Domain: `Domain.Tools`. Services: `IToolService`,
`IToolQueries`. AI suggestions via `IToolAiAssistantService` (human apply only).
SoftwareApplication structured-data DTO is foundation-only (no public injection).
Search remains Content-event driven — no separate tool search engine.

See also: `docs/admin/tool-library-platform-v1.md`.

## Roadmap Engine extension (Sprint 49)

Satellite tables `roadmap_metadata` (+ `roadmap_steps`, `roadmap_topics`,
`roadmap_resources`) hang off `Content` when `ContentType=Roadmap`. Domain:
`Domain.Roadmaps`. Services: `IRoadmapService`, `IRoadmapQueries`. Resources link to
other modules by identifier string only (no hard FK). AI suggestions via
`IRoadmapAiAssistantService` (human apply only). LearningRoadmap / Course structured-data
DTO is foundation-only. User progress is intentionally not implemented
(`IUserRoadmapProgressFoundation` marker).

See also: `docs/admin/roadmap-engine-v1.md`.

## Public API (unchanged)

| Method | Route | Auth | Notes |
| --- | --- | --- | --- |
| GET | `/api/v1/content` | Public | Published content list |
| GET | `/api/v1/content/{slug}` | Public | Published content by slug |
| POST | `/api/v1/content` | WriterOrAdmin | Create content |

These routes, request shapes and response DTOs (`ContentListItemDto`, `ContentDetailDto`)
are preserved exactly.

## Admin API (new)

Routed under `/api/v1/admin/content` by `ContentManagementController`. Policy:
`WriterOrAdmin` with ownership enforcement (see below).

| Method | Route | Purpose |
| --- | --- | --- |
| GET | `/api/v1/admin/content?page=&pageSize=&search=&status=&type=` | Paged admin list |
| GET | `/api/v1/admin/content/{id}` | Admin detail read model (drafts + SEO + timestamps) |
| PUT | `/api/v1/admin/content/{id}` | Update existing content |
| POST | `/api/v1/admin/content/{id}/publish` | Publish a draft (Draft → Published) |
| PUT | `/api/v1/admin/content/{id}/seo` | Update SEO metadata |
| GET | `/api/v1/admin/content/{id}/article` | Get ArticleMetadata (204 if none) |
| PUT | `/api/v1/admin/content/{id}/article` | Upsert ArticleMetadata |
| GET | `/api/v1/admin/content/{id}/news` | Get NewsMetadata (204 if none) |
| PUT | `/api/v1/admin/content/{id}/news` | Upsert NewsMetadata |
| GET | `/api/v1/admin/content/{id}/tool` | Get ToolMetadata (204 if none) |
| PUT | `/api/v1/admin/content/{id}/tool` | Upsert ToolMetadata (+ alternatives) |
| POST | `/api/v1/admin/content/{id}/tool/features` | Add ToolFeature |
| DELETE | `/api/v1/admin/content/{id}/tool/features/{featureId}` | Remove ToolFeature |
| GET | `/api/v1/admin/tools` | List tool library rows (ownership-scoped) |
| GET | `/api/v1/admin/content/{id}/roadmap` | Get RoadmapMetadata (204 if none) |
| PUT | `/api/v1/admin/content/{id}/roadmap` | Upsert RoadmapMetadata |
| POST | `/api/v1/admin/content/{id}/roadmap/steps` | Add RoadmapStep |
| PUT | `/api/v1/admin/content/{id}/roadmap/steps/{stepId}` | Update RoadmapStep |
| DELETE | `/api/v1/admin/content/{id}/roadmap/steps/{stepId}` | Remove RoadmapStep |
| POST | `/api/v1/admin/content/{id}/roadmap/steps/reorder` | Reorder steps |
| GET | `/api/v1/admin/content/{id}/revisions` | Paginated revision history (newest first) |
| GET | `/api/v1/admin/content/{id}/revisions/{version}` | Immutable revision snapshot |
| POST | `/api/v1/admin/content/{id}/revisions/{version}/restore` | Restore snapshot; appends new revision |
| POST | `/api/v1/admin/content/{id}/seo-analysis` | Deterministic SEO analysis (saved content; no persistence) |

> Note: the controller class is named `ContentManagementController` (not
> `AdminContentController`) because the API architecture guardrail reserves the `Admin`
> class-name prefix for `AdminOnly` controllers; content management intentionally uses the
> `WriterOrAdmin` policy. The **route** is still `/api/v1/admin/content` as specified.

### List query

- `AdminContentListItemDto`: `Id, Title, Slug, ContentType, ContentStatus, AuthorId,
  CreatedAtUtc, UpdatedAtUtc, PublishedAtUtc`.
- `ContentSearchFilter` normalizes input: default `page=1`, `pageSize=20`, max
  `pageSize=100`; blank filters become null.
- `AdminContentQueries` runs `AsNoTracking` + projection with pagination and ordering in
  SQL. Deterministic order: `UpdatedAt` descending, then `Id` ascending. Search is a
  case-insensitive title match plus exact slug match.

### Admin read model (detail)

The Admin CMS / Content Studio needs a full, private view of a single item — including
drafts, SEO metadata (before it is ever public) and management timestamps — without
depending on the public slug endpoint (which only returns published items and omits SEO).
`GET /api/v1/admin/content/{id}` provides this dedicated read model.

- **Contract** — `IAdminContentQueries` (Application) exposes, alongside `ListAsync`:
  - `GetByIdAsync(Guid id, …)` → `AdminContentDetailDto?`
  - `GetBySlugAsync(string slug, …)` → `AdminContentDetailDto?` (for future systems;
    returns `null` for an invalid/unknown slug)
- **DTO** — `AdminContentDetailDto`: `Id, Title, Slug, Body, Excerpt, CoverImage,
  ContentType, ContentStatus, AuthorId, Views, Saves, CreatedAtUtc, UpdatedAtUtc,
  PublishedAtUtc, Seo` (a nested `SeoMetadataDto`). It exposes **no** Domain/EF entities or
  navigation properties — only primitives and the SEO sub-DTO. This is the same DTO already
  returned by the update/publish/SEO mutations, so the read and write shapes match.
- **Query architecture** — `AdminContentQueries.GetByIdAsync/GetBySlugAsync` use
  `AsNoTracking` and a `Select` projection into a private `DetailRow` (value objects and
  enums are materialized then mapped in memory, exactly like the list query), followed by
  `FirstOrDefaultAsync`. This is a **single SQL round-trip**, projection-only, with **no
  aggregate tracking**, **no `Include`/N+1**, and **no `IQueryable` escaping** the method.
  The SEO columns are read via the `SeoMetadata` complex property (`content.SeoMetadata.…`),
  so `contents` + SEO columns map directly to the DTO.
- **Ownership / security** — the query itself is ownership-agnostic (a pure read). The
  Application service `IContentService.GetManagedByIdAsync(actor, id)` loads the projection
  and applies the **same 404-masking** as update/publish: a missing item, or a **writer**
  requesting another author's content, both throw `content_not_found` (404). Existence is
  never leaked. Admins can read all content.
- Because the read model owns full detail (including drafts and SEO), the Admin CMS no
  longer needs the public `GET /content/{slug}` endpoint for editing.

### Update / Publish

- Request `UpdateContentRequest`: `Title, Slug, Type, Body` (+ optional `Excerpt`,
  `CoverImage`). Slug follows `^[a-z0-9]+(?:-[a-z0-9]+)*$`, length 2–300; duplicate slugs
  are rejected with `409`.
- `Publish` is idempotent: publishing an already-published item is a no-op.

### SEO metadata

SEO metadata is modelled as a `SeoMetadata` value object on the `Content` aggregate and is
**admin/editor-only** — it is never exposed through the public content API.

- Value object fields (all optional, normalized: trimmed, blanks → `null`):
  - `SeoTitle` (max 70)
  - `SeoDescription` (max 160)
  - `CanonicalUrl` (must be an absolute `http`/`https` URL if provided; max 2048)
  - `OgImage` (max 2048)
  - `FocusKeyword` (max 100)
- Invalid values throw `DomainException`, surfaced by the service as
  `content_invalid_operation` (`409`).
- `PUT /api/v1/admin/content/{id}/seo` takes `UpdateSeoMetadataRequest` and returns
  `AdminContentDetailDto`, whose `Seo` member is a `SeoMetadataDto`
  (`seoTitle, seoDescription, canonicalUrl, ogImage, focusKeyword`).
- `Content.UpdateSeoMetadata(seoMetadata, updatedAtUtc)`:
  - No-op when the metadata is unchanged (value equality) — `UpdatedAt` is not bumped and
    no event is raised.
  - On a **draft**, updates silently (no domain event).
  - On a **published** item, bumps `UpdatedAt` and raises `ContentUpdatedDomainEvent`
    (reusing `content.updated.v1`), so the Outbox drives a Search projection refresh.

The **public** `ContentDetailDto` deliberately does not expose SEO fields; the public API
remains `title`, `body`, `excerpt`, `cover image` only.

## CMS lifecycle

```
Create (Draft)  ──►  Update (Draft, silent)  ──►  Publish  ──►  Update (Published, event)
                                                     │
                                                     └─► sets PublishedAtUtc
```

- `UpdatedAt` is bumped only when a field actually changes.
- `PublishedAtUtc` is set once, on the Draft → Published transition.

## Domain events → Outbox → Search

The aggregate raises domain events which the shared `ApplicationDbContext.SaveChangesAsync`
captures transactionally into the Outbox. The Outbox processor later dispatches them to
in-process handlers.

| Domain event | Outbox type | Raised when |
| --- | --- | --- |
| `ContentPublishedDomainEvent` | `content.published.v1` | Draft → Published |
| `ContentUpdatedDomainEvent` | `content.updated.v1` | A **published** item changes |

Draft edits are intentionally silent (drafts are never in public read models / search).
The Search module consumes both events (`ContentPublishedSearchHandler`,
`ContentUpdatedSearchHandler`) and refreshes its projection by re-reading the content
source, which returns `null` for non-published items. No new module dependency is
introduced and Search remains purely event-driven.

## Ownership rules

Authorization uses the `WriterOrAdmin` policy; fine-grained ownership is enforced in the
Application layer (never in the Domain):

- **Writer** – may list, update and publish **only their own** content
  (`AuthorId == UserId`).
- **Admin** – may manage **all** content.

The API builds a framework-neutral `ContentManagementActor(userId, canManageAllContent)`
from claims (`canManageAllContent = IsInRole(Admin)`). Cross-owner access is reported as
`content_not_found` (404) so writers cannot probe other authors' content.

## Metadata fields

`AddContentCmsFieldsV1` adds to `contents`:

- `excerpt` (`varchar(500)`, not null, default `''`)
- `cover_image` (`varchar(2048)`, nullable)
- `updated_at` (`timestamptz`, not null; indexed for list ordering)
- `published_at_utc` (`timestamptz`, nullable)

The migration backfills existing rows: `updated_at = created_at`, and
`published_at_utc = created_at` for already-published rows. No other tables are changed.

`AddContentSeoMetadataV1` adds the SEO columns to `contents` (all nullable, no other table
changed): `seo_title` (`varchar(70)`), `seo_description` (`varchar(160)`), `canonical_url`
(`varchar(2048)`), `og_image` (`varchar(2048)`), `focus_keyword` (`varchar(100)`). They are
mapped via an EF Core complex property so the always-present `SeoMetadata` value object maps
inline onto the same table; existing content stays valid with all-null SEO columns.

## Error handling

`ContentException` carries a `Code`; `ContentExceptionFilter` maps codes to HTTP status:

| Code | Status |
| --- | --- |
| `content_not_found` | 404 |
| `content_slug_duplicate` | 409 |
| `content_invalid_operation` | 409 |
| `content_validation_failed` | 400 |

## SEO Analyzer Engine (v1)

Authoritative, deterministic, rule-based SEO analysis lives in
`HelpDev.Modules.Content.Application.SeoAnalysis`. It is **pure** (no EF, HTTP,
network, AI, or external SEO SDK) and **ephemeral** (no persistence, no Outbox,
no migration).

### Architecture

- `IContentSeoAnalyzer` / `ContentSeoAnalyzer` — runs an explicitly ordered list of
  `ISeoAnalysisRule` implementations (no reflection plugins).
- `SeoAnalysisInput` — immutable DTO mapped from the Admin read model (never a
  Domain/EF entity).
- `MarkdownDocumentScanner` — bounded Markdown facts (headings, paragraphs,
  fenced code, links, word/keyword counts). HTML is never executed; URLs are
  never fetched.
- Rules reuse a single `MarkdownDocumentFacts` scan per analysis (linear in body
  length).
- Recommendation constants live in `SeoAnalysisOptions` (editorial guidance —
  **not** Google ranking guarantees). Domain hard maxima (e.g. SeoTitle ≤ 70)
  remain in the Domain VO.

### Report model (SEO Platform v1)

API responses use `SeoAuditReportDto`: `ContentId`, `GeneratedAtUtc`, `Summary`
(`ErrorCount`, `WarningCount`, `InfoCount` only), and `Findings`.

Each `SeoAuditFindingDto` uses platform categories (`Metadata`, `ContentStructure`,
`Images`, `Links`, `Technical`), `RuleId`, `Severity`, `Message`, `Suggestion`,
and optional `Field` (e.g. `seoTitle`, `canonicalUrl`).

The rule engine still produces internal `SeoAnalysisReportDto` (with statistics for
tests); `SeoAuditMapper` projects to the platform contract. There is **no** SEO score,
percentage, ranking prediction, keyword volume, or domain authority.

Each finding has a stable language-neutral `RuleId` (e.g. `seo.title.missing`,
`seo.link.no_internal`, `seo.canonical.missing`).

### Heading convention

The Content title renders as the page H1 **outside** the Markdown body
(`ContentDetailsCard` / Studio preview). Body H1 is therefore discouraged;
prefer `##` (H2) and below (`seo.heading.body_h1`).

### Endpoint

| Method | Path | Notes |
| --- | --- | --- |
| POST | `/api/v1/admin/content/{id}/seo-analysis` | Explicit on-demand computation (chosen over GET because analysis is a command-style computation, even though it is side-effect free) |

- Auth: `WriterOrAdmin`
- Ownership: Writer = own content only; Admin = all; cross-owner Writer →
  `404 content_not_found` (no existence leak)
- Analyzes **saved server content** only
- Responses: 200 report, 401, 403 (policy), 404, 500 safe problem + correlation id
- No `SaveChanges`, no domain event, no Outbox message

Application entry: `IContentService.AnalyzeSeoAsync(actor, id, ct)`.

### SEO health dashboard (v1)

| Method | Path | Notes |
| --- | --- | --- |
| GET | `/api/v1/admin/seo/dashboard` | Aggregate counts from stored content (missing title/description/cover/canonical). `LastAnalysisTime` is `null` until analysis history is persisted. |

`ISeoDashboardQueries` runs in Content Infrastructure (scoped by Writer ownership).
No SEO tables or migrations in v1.

Admin UI: `/admin/seo` (تحلیل SEO).

### Structured data foundation (v1)

`IStructuredDataGenerator` / `ArticleSchemaDto` — JSON-LD shaped DTO only (`schema.org`
Article). Not injected into public pages in v1.

### Internal link suggestions (v1)

`IInternalLinkSuggestionService` — implementation in `HelpDev.Infrastructure` using
`ISearchQueries` (`sourceType=content`). No AI, embeddings, or automatic link insertion.
Content.Application does not reference the Search module.

### Rule IDs (v1)

Title / description / keyword / slug / headings / content length / first
paragraph / links / media / canonical / code — see
`Application/SeoAnalysis/Rules/*`. Examples: `seo.title.length`,
`seo.keyword.coverage`, `seo.heading.level_jump`, `seo.link.unsafe_scheme`,
`seo.code.language_missing`.

### Future analyzer versioning / AI advisory

- Persist reports only with an explicit analyzer version field (not in v1).
- An optional AI advisory layer may sit **beside** this engine later; it must
  never replace deterministic findings or invent scores/volumes.

## Future SEO extension points

- **Search indexing of SEO fields** – a later sprint may extend the Search projection to
  index `SeoTitle`/`FocusKeyword`. Today the Search schema is unchanged and SEO fields are
  not indexed; the projection still refreshes on `content.updated.v1`.
- **Additional fields** – new SEO members (e.g. `Robots`, `TwitterCard`) can be added to
  `SeoMetadata` + one nullable column each, following `AddContentSeoMetadataV1`.

## Editorial workflow (v1)

State machine (`ContentStatus` stored as string on `contents.status`):

| State | Meaning |
| --- | --- |
| `Draft` | Author editing |
| `ReviewPending` | Awaiting moderation |
| `Approved` | Ready to publish |
| `Published` | Public + search index |
| `Archived` | Retired (not in public list) |

Allowed transitions: Draft→ReviewPending; ReviewPending→Draft (reject) or Approved;
Approved→Published; Published→Archived.

Immutable audit rows in `content_workflow_history`. Only **Approved→Published** raises
`ContentPublishedDomainEvent` / `content.published.v1`.

### Roles (application layer)

| Action | Writer (own content) | Admin |
| --- | --- | --- |
| Submit for review | ✓ | ✓ |
| Approve / Reject | | ✓ |
| Publish / Archive | | ✓ |

There is no separate `Editor` Identity role in v1; **Admin** covers editor duties.

### Admin API

| Method | Route |
| --- | --- |
| POST | `.../submit-review` |
| POST | `.../approve` |
| POST | `.../reject` |
| POST | `.../publish` |
| POST | `.../archive` |
| GET | `.../workflow-history` |

Create-with-`Published` still works: service records Submit→Approve→Publish history via
`BootstrapPublishAfterCreateAsync`.

### Limitations

- Archive does not remove search documents (no outbox event).
- Reject requires a non-empty comment.

## Revision history (v1)

Immutable per-content snapshots stored in `content_revisions` (`snapshot_json` jsonb). No
update/delete APIs; restore never removes prior rows.

### Snapshot strategy

`ContentRevisionSnapshot` is a versioned value object (title, slug, body, excerpt,
cover image, content type, nested SEO fields). It is **not** the live `Content` aggregate
and can evolve independently. Revisions capture the **post-change** state after a
successful content update, SEO update, or restore.

### When revisions are created

- After a successful `UpdateAsync` that actually changes fields (no-op updates skip).
- After a successful `UpdateSeoMetadataAsync` that changes SEO (equality no-op skips).
- After a successful `RestoreAsync` (always appends a new revision, including change reason).

Creation uses the same `IUnitOfWork` transaction as the content mutation and outbox
capture. Failed validation, unauthorized access, and no-op updates do **not** create rows.

### Restore

`POST …/revisions/{version}/restore` loads the snapshot, applies it via
`Content.RestoreFromSnapshot`, appends the next sequential version, and raises
`ContentUpdatedDomainEvent` when the item is **Published** (search/outbox unchanged).

### Admin API

| Method | Route |
| --- | --- |
| GET | `/api/v1/admin/content/{id}/revisions` |
| GET | `/api/v1/admin/content/{id}/revisions/{version}` |
| POST | `/api/v1/admin/content/{id}/revisions/{version}/restore` |

Ownership matches other admin content endpoints (`content_not_found` masking).

### Limitations (v1)

- No revision on initial create or publish-only transitions.
- No diff storage; compare is client-side field comparison.
- No retention/pruning jobs.
- Draft edits create revisions but do not emit `content.updated.v1` until published.

## Content Analytics Platform (v1)

Content analytics **reuses** the Analytics module (`analytics_daily_metrics`). No
`content_analytics_metrics` table and **no new migration** in Sprint 37.

### Real producers (do not invent)

| Signal | Source |
| --- | --- |
| Views | `content.item_viewed` on public slug GET → `content.views` |
| Created | `content.item_created` on create |
| Published | Outbox `ContentPublishedDomainEvent` → analytics handler |

**Not produced for Content:** Favorite, Save, Share, SearchClick, Completion.
`ContentMetricType` reserves those enum values but they are unsupported until real
producers exist.

### Admin APIs

| Method | Path |
| --- | --- |
| GET | `/api/v1/admin/analytics/content` |
| GET | `/api/v1/admin/analytics/content/{id}` |
| GET | `/api/v1/admin/analytics/top-content` |
| GET | `/api/v1/admin/analytics/content-health` |

Auth: `AdminOnly`. Health indicators use update age, revision count, SEO metadata gaps,
and period views — **no numeric score**.

### Admin UI

- `/admin/analytics/content` — content analytics dashboard
- `/admin/content/{id}/analytics` — per-item Analytics tab

