# Content Workspace Architecture v1

**Sprint:** 46.6 — Content Architecture Refactor v1  
**Scope:** Admin CMS UX only (frontend). No backend API, migration, or Content aggregate changes.

---

## Why the generic editor was replaced

The previous flow (`/admin/content/new`) forced authors through a **generic** form with a
manual **Content Type** selector (Article / News / Tool / …). That model:

- Collapsed distinct content domains into one undifferentiated editor
- Encouraged inventing UI fields that the backend does not support
- Made navigation and mental models harder for writers and admins

HelpDev content domains need **specialized workspaces** while still posting to the same
`CreateContent` / admin list APIs.

---

## Workspace model

```
محتوا (Admin)
├── مقالات      → ContentType.Article
├── اخبار       → ContentType.News
├── ابزارها     → ContentType.Tool      (content pages; Toolbox module stays separate)
├── نقشه راه    → ContentType.RoadmapStep
└── Prompt Lab  → delegates to /admin/prompt-lab (no duplicated Prompt entities)

آموزش (separate)
├── دوره‌ها
└── درس‌ها (future)
```

### Factory

`src/lib/admin/content/factory/content-type-registry.ts` maps workspace keys to:

- Backend `ContentType` string
- List / create routes
- Copy (titles, descriptions)
- Optional `delegatesToPromptLab` / `futureCapabilities`

Editors are resolved via `WORKSPACE_EDITORS` — **no duplicated API clients**.

---

## Routes

| Route | Role |
|-------|------|
| `/admin/content/articles` | Article list (type filter locked) |
| `/admin/content/articles/new` | Article create (type fixed) |
| `/admin/content/articles/[id]` | Redirect → `/admin/content/[id]` |
| `/admin/content/news` (+ `/new`) | News workspace |
| `/admin/content/tools` (+ `/new`) | Tool content foundation |
| `/admin/content/roadmaps` (+ `/new`) | RoadmapStep foundation |
| `/admin/content/prompts` (+ `/new`) | Link to Prompt Lab |
| `/admin/content/new` | **Redirect** → articles/new |
| `/admin/content` | Legacy “all content” list (kept) |

---

## Current capabilities

| Workspace | Saved via API | Notes |
|-----------|---------------|-------|
| Article | title, slug, body, type=`Article`, status | SEO/media in existing Studio after create |
| News | same with type=`News` | |
| Tool | same with type=`Tool` | Future tool schema fields shown as disabled |
| Roadmap | same with type=`RoadmapStep` | Structured steps = future |
| Prompt | — | Links to Prompt Lab only |

Shared UX: `WorkspaceHeader`, `WorkspaceEmptyState`, `WorkspaceStats`, RTL Admin design system.

---

## Future extensions

- Structured roadmap steps / dependencies
- Tool input schema + Toolbox run binding
- News-specific metadata if backend adds fields
- Prompt content bridging only if product decides Content∪PromptLab merge

Until the API exposes fields, UI must show **«در نسخه آینده»** and must **not** fake save.

---

## Explicit non-goals (this sprint)

- Database migrations
- New/changed backend endpoints
- Content aggregate redesign
- Fake fields or unsupported workflows
