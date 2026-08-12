# Admin Content CMS Engine v1

`/admin/content` is the HelpDev CMS workspace: list, search/filter, create,
preview, details, Content Studio and SEO — built on the **existing** Content
Admin APIs. No invented fields; backend authorization remains authoritative.

> This document reflects the **real** backend surface (inspected from
> `HelpDev.Modules.Content`). See [Current limitations](#current-limitations).

---

## 1. Architecture

```
src/lib/admin/content/          ← data layer (no JSX)
  content-types.ts              ← raw DTOs (mirror backend) + view models + form types
  content-api.ts                ← adapters over the shared @/lib/api/content client
  content-mappers.ts            ← pure mapping, slugify, validation (content + SEO) (tested)
  content-url-state.ts          ← URL-driven workspace query parse/serialize (tested)
  content-analyzer.ts           ← factual statistics + quality report (no score) (tested)
  editor-draft.ts               ← local-only draft recovery (no secrets) (tested)
  markdown.ts                   ← minimal, safe Markdown parser (tested)
  content-hooks.ts              ← useAdminContentList / useContentStats /
                                  useAdminContentDetail / useCreateContent /
                                  useUpdateContent / usePublishContent / useUpdateSeoMetadata

src/components/admin/content/   ← presentation layer
  content-dashboard.tsx         ← /admin/content workspace (URL state + admin list)
  list/
    content-toolbar.tsx         ← debounced search + filters (AdminActionBar)
    content-filters.tsx         ← type + status selects (server-side)
    content-table.tsx           ← responsive table (desktop) / cards (mobile)
    content-status-badge.tsx
    content-empty-state.tsx     ← global / filtered / writer-scoped empty
  editor/ …                     ← create + Content Studio (unchanged routes)
  seo/ …                        ← SEO workspace
  details/ …                    ← id-based detail tabs
  shared/
    content-type-badge.tsx
    content-actions.tsx         ← View / Edit (id) + inline Draft publish
    markdown-preview.tsx

src/components/admin/shared/
  admin-pagination.tsx          ← reusable prev/next + page-size selector
```

Principles:

- All network access lives in `lib/admin/content` — components never fetch
  directly; the shared typed API client (`@/lib/api/content`) is reused.
- The Admin workspace list uses **`GET /api/v1/admin/content` only** — never the
  public published-only `GET /content`.
- Mapping, URL state, validation and Markdown parsing are **pure and unit-tested**.
- UI reuses Admin design-system primitives plus `AdminPagination`.

---

## 2. Routes

| Route                        | Purpose                | Notes |
| ---------------------------- | ---------------------- | ----- |
| `/admin/content`             | CMS workspace / list   | URL-driven filters + server pagination; drafts + published |
| `/admin/content/new`         | Create content         | fully functional (POST) |
| `/admin/content/[id]`        | Details / preview      | `[id]` = **content id (Guid)**; tabs: Overview / Editor / SEO |
| `/admin/content/[id]/edit`   | **Content Studio**     | 3-column editor + SEO workspace + preview |

**The route param is the content id (Guid).** List rows, tabs, edit/back buttons
and create-redirect all use the id. Admin detail/Studio load
`GET /api/v1/admin/content/{id}`. There is no public per-slug content page on the
site yet, so the workspace does **not** invent a public preview URL.

Example shareable workspace URL:

`/admin/content?page=2&pageSize=20&search=cursor&status=Draft&type=Article`

Supported query keys: `page`, `pageSize`, `search`, `status`, `type`. Invalid
values fall back safely (page→1, pageSize→20, unknown enums→all). Empty filters
are omitted from the serialized URL. Browser back/forward works via
`useSearchParams` + `router.replace` (no full reload).

All content pages live under `/admin`, so they inherit the Admin Shell's
`AdminGuard` (Admin-only UI gate). The backend remains the authority for data
access (WriterOrAdmin + ownership on the list/detail endpoints).

---

## 3. API sources used

| Operation        | Endpoint                                    | Status |
| ---------------- | ------------------------------------------- | ------ |
| **Admin list**   | `GET /api/v1/admin/content`                 | ✅ paged; drafts + published; ownership-scoped |
| Admin detail     | `GET /api/v1/admin/content/{id}`            | ✅ Studio/details (drafts + SEO + timestamps) |
| Create content   | `POST /api/v1/content`                      | ✅ Draft or Published |
| Content stats    | `GET /api/v1/admin/dashboard`               | ✅ global aggregates for stat cards |
| Update content   | `PUT /api/v1/admin/content/{id}`            | ✅ Writer/Admin, ownership enforced |
| Publish existing | `POST /api/v1/admin/content/{id}/publish`   | ✅ Draft → Published (inline + Studio) |
| Update SEO       | `PUT /api/v1/admin/content/{id}/seo`        | ✅ separate save |
| **SEO analysis** | `POST /api/v1/admin/content/{id}/seo-analysis` | ✅ on-demand; saved server content; no persistence |
| **Media library** | `GET/POST /api/v1/admin/media`, `GET .../{id}` | ✅ images only; see [admin-media-library.md](./admin-media-library.md) |

**Not used by the Admin workspace:** `GET /api/v1/content` (public published-only list).

Real list contract (inspected):

- Query: `search`, `status`, `type`, `page` (default 1), `pageSize` (default 20, max 100).
- Response `PagedResult`: `items`, `page`, `pageSize`, `totalCount`, `totalPages`.
- Item `AdminContentListItemDto`: `id, title, slug, contentType, contentStatus,
  authorId, createdAtUtc, updatedAtUtc, publishedAtUtc` (no body/SEO/views/cover).
- Writer: list scoped to own `AuthorId`. Admin: all content. Cross-owner detail
  access returns `404 content_not_found` (no existence leak).

Other DTO shapes:

- `AdminContentDetailDto` (GET by id + mutations): includes `body`, `excerpt`,
  `coverImage`, SEO, timestamps.
- `ContentType`: `News, Article, RoadmapStep, Tool, Prompt, Course`.
- `ContentStatus`: `Draft, Published`.
- Slug rule: `^[a-z0-9]+(?:-[a-z0-9]+)*$`, length 2–300.

> The Content Studio and details page load exclusively from the Admin Read Model
> (`GET /admin/content/{id}`, via `getAdminContentById` → `useAdminContentDetail`),
> so excerpt/cover/SEO/timestamps are pre-filled for drafts and published items.
> Ownership is enforced server-side (a writer requesting another author's content
> gets `404 content_not_found`). See [Content Studio](#9-content-studio-sprint-29).

---

## 4. Editor capabilities

- **Two-column desktop layout**: left = editor (Title, Slug, Type, Body); right =
  live Markdown **preview** + **PublishPanel**. Stacks vertically on mobile.
- **Slug helper**: auto-generated from the title (until manually edited) and a
  “generate from title” button; validated against the exact backend slug rule.
- **Markdown body**: a dependency-free editor (textarea) with a safe live
  preview. Supported: headings (1–3), paragraphs, fenced code blocks, ordered /
  unordered lists, and inline **bold**, *italic*, `code`, and links. Links with
  unsafe protocols are rendered as plain text; the preview never uses
  `dangerouslySetInnerHTML`, so content cannot inject markup.
- **Create** persists via `POST /content`. **Save Draft** sends
  `status = "Draft"`; **Publish** sends `status = "Published"` after a confirm
  dialog. On success the user is redirected to `/admin/content/{id}`.
- **Edit** (Content Studio) loads via `GET /admin/content/{id}` and saves via
  `PUT /admin/content/{id}` / `PUT .../seo`. Inline list **Publish** uses
  `POST /admin/content/{id}/publish` with confirmation; on success the list
  refetches (and steps back a page if the last Draft on a non-first page
  disappeared from a Draft filter).

---

## 5. States, RTL, theme, responsive

- **UI states**: every data view supports Loading (`AdminLoadingState`), Error
  (`AdminErrorState`, safe message + `correlationId`, never stack traces/raw
  payloads), Empty (`AdminEmptyState` / `ContentEmptyState`), and Success.
- **RTL**: logical CSS throughout (`text-start`, `ps-*`, `justify-self-end`);
  tables, forms, dialogs, badges and actions all flow right-to-left. LTR is
  applied only to inherently LTR data (slugs, code, author ids).
- **Theme**: all colors use `--adm-*` tokens under `data-admin-theme`
  (light/dark); no hardcoded colors.
- **Responsive**: list renders a table at `md+` and stacked cards below `md`
  (no horizontal scrolling). The editor and details grids collapse to a single
  column on mobile. Verified conceptually at 375 / 768 / 1024 / 1440 / 1920px.
- **Accessibility**: labeled inputs, `aria-invalid` on errors, focus styles
  (`adm-focus`), and an accessible publish confirm dialog (`role="dialog"`,
  `aria-modal`, `aria-labelledby`, Escape to close, focus moved to confirm).

---

## 6. Performance

- Admin list and dashboard stats are fetched independently; list requests are
  aborted on query change / unmount. Filter refreshes keep prior rows visible
  (`refreshing`) to avoid a full-page flash.
- Detail fetches are keyed by content id and aborted on navigation.
- Search is debounced (~400ms) before updating the URL (no request per keystroke).
- No new dependencies were added (no data-table / Markdown editor package).

---

## 7. Current limitations

- **Workspace list is admin-backed** → `GET /admin/content` returns drafts and
  published items with server-side search/status/type/pagination. Ownership
  scoping is enforced by the backend (Writer = own; Admin = all).
- **Statistics are honest** → global Total / Published / Draft come from the
  Admin Dashboard aggregates. «نتایج فیلتر فعلی» is the list response
  `totalCount` for the active filters — never a count of the current page.
  There is no per-page fake global total.
- **No author name** — only `AuthorId` is exposed; shown shortened (tooltip has full id).
- **No public slug preview route** on the site yet — the workspace does not invent
  a public URL. Admin view/edit always use id routes. Drafts never go through
  the public slug endpoint.
- **Analyzer is factual only** — no SEO score, grade, or AI judgement.

---

## 8. Future extensions

- **Public content page by slug** — when a public route exists, Published rows
  can offer a safe public preview link (slug-based). Drafts must stay admin-only.
- **Media library** — Sprint 33: `/admin/media` + picker for `CoverImage` / `OgImage` (URL compatibility preserved). See `site/docs/admin/admin-media-library.md`.
- **Revision history** — versions, diffs, restore.
- **Scheduling** — a `Scheduled` status and publish-at datetime.
- **Author filter / lookup** — only if the backend exposes it (do not invent).

### Future AI extension points (not implemented)

The analyzer and SEO workspace are deliberately AI-free. If AI assistance is
added later, it should plug into the existing seams **without** changing the
factual analyzer or fabricating scores:

- **Suggestion adapter** — a new `lib/admin/content/ai/*` module behind a server
  endpoint (never a client-side provider key) could propose an SEO title,
  description, or focus keyword. The UI would present these as opt-in
  suggestions next to `SeoField`, leaving the user's real values authoritative.
- **Analyzer stays factual** — any AI "quality" output must be clearly separated
  from `ContentAnalyzer`/`ContentQualityPanel`, which remain pure measurements.
- **No secrets client-side** — provider credentials must live server-side; the
  frontend would call a backend proxy, consistent with the shared API client.

---

## 9. Content Studio (Sprint 29)

`/admin/content/[id]/edit` is the **Advanced Content Studio** — a focused writing
and SEO workspace built on the same primitives, with **no new dependencies** (no
rich-text/WYSIWYG library, no AI provider).

### Layout

- **Desktop (≥ xl)**: three columns — **left** document outline, **center**
  editor/preview, **right** SEO + analysis. **Mobile**: a single column stacks
  Editor → SEO → analysis (outline last). Fully responsive, no horizontal scroll.

### Components (`components/admin/content/editor` + `.../seo`)

- `ContentStudio` — orchestrator (state, dirty tracking, saves, draft recovery).
- `MarkdownEditor` + `EditorToolbar` — textarea with a selection-aware Markdown
  toolbar (bold/italic/code/link/heading/list). Dependency-free.
- `ContentOutline` — live heading outline from the safe Markdown parser.
- `ContentPreviewPanel` — live article preview with **desktop/mobile** framing;
  renders the safe Markdown tree (never `dangerouslySetInnerHTML`).
- `ContentStatisticsCard` — words, characters, headings, code blocks, links, and
  a labelled reading-time **estimate** (`words / 200`, min 1).
- `ContentQualityPanel` — neutral checklist from `ContentAnalyzer` (no score).
- `SaveStatusIndicator` — a single quiet pill: `saving / saved / unsaved / error`
  (no toast spam).
- `SeoPanel` + `SeoField` + `SeoGooglePreview` + `SeoSocialPreview` — the SEO
  workspace with its **own** save button (separate from content save).
- `SeoAnalysisPanel` — deterministic backend findings (Sprint 32). Labelled
  «تحلیل براساس آخرین نسخه ذخیره‌شده». Summary shows Passed / Warnings /
  Errors / Informational counts only — **no** overall SEO score or percentage.

### SEO analysis UI (Sprint 32)

- Triggered by an explicit **Analyze / Rerun** button (`useContentSeoAnalysis`).
  Never runs on each keystroke.
- Analyzes the **last saved server version** via
  `POST /admin/content/{id}/seo-analysis`. Local unsaved drafts are not sent.
- Lifecycle: `idle → analyzing → success | error`, and **stale** when content or
  SEO fields change after a report, or after a successful content/SEO save.
  Stale reports stay visible but clearly marked; a single optional auto-rerun
  may run after save only if the author had already analyzed (no loops).
- Findings are grouped (Metadata, Keyword, Structure, Links, Media, Content /
  code). Each finding shows status text, title, message, evidence, and
  recommendation.
- Categories are editorial groupings of deterministic rules — not ranking claims.
- Limitations: no keyword volume, competition, domain authority, broken-link
  network checks, image dimensions, or AI rewriting.

### Saves

- **Content save** → `PUT /admin/content/{id}` (title, slug, type, body, excerpt,
  cover image). Disabled until there are unsaved changes.
- **SEO save** → `PUT /admin/content/{id}/seo` (separate button + status).
- **Publish** → `POST /admin/content/{id}/publish` (shown for drafts).
- All go through the shared typed client (`@/lib/api/content`); errors surface via
  `AdminErrorState`/`ApiClientError` (safe messages + correlation id).

### Local draft recovery

- Key `helpdev.content.editor.draft.v1`; stores **only** `contentId, title, body,
  excerpt, timestamp`. **No token, no JWT, no user data.** SSR-guarded and
  malformed-safe (corrupt entries are cleared). This is local-only recovery — it
  is **not** server autosave. Cleared on a successful content save or discard.

### Google & social previews

- Purely visual. `SeoGooglePreview` uses `seoTitle` (else the content title) and
  `seoDescription` (else the excerpt); `SeoSocialPreview` uses `ogImage` (else the
  cover image). No Google/OG API is ever called.
