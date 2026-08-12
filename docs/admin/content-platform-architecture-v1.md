# Content Platform Architecture v1

**Sprint:** 47A — HelpDev Content Platform Architecture Refactor  
**Scope:** Frontend Admin CMS only. No backend, migrations, or API inventing.

---

## Core idea

`/admin/content` is the **Content Platform hub**: a navigation surface to specialized
workspaces. Authors no longer start from a generic type selector.

```
/admin/content                    → platform hub
/admin/content/all                → legacy full Content list (API)
/admin/content/articles|news/...  → workspaces
```

---

## Workspace architecture

Registry: `src/lib/admin/content/registry`

Each entry:

| Field | Meaning |
|-------|---------|
| `id` | Stable workspace id |
| `title` / `shortTitle` | Persian labels |
| `route` / `createRoute` | Admin URLs |
| `icon` | Admin icon name |
| `persistence` | `content-api` \| `prompt-lab` \| `none` |
| `contentType` | Backend enum when `content-api` |

UI wiring: `WORKSPACE_EDITORS` / `WORKSPACE_LISTS` in
`components/admin/content/workspaces/workspace-editors.ts`.

Shared chrome: Header, Stats, Empty state, Create affordance (foundation shells for
non-persisted workspaces).

---

## Persistence matrix

| Workspace | Persistence | Notes |
|-----------|-------------|-------|
| Articles | Content API | Type locked `Article`; SEO/media via Studio |
| News | Content API | Type locked `News` |
| Tools | **none** | Name / Website / Category / Features — UI only |
| Roadmaps | **none** | Builder: title, description, steps, topics, resources |
| Prompt | Prompt Lab | Link only — no duplicated prompt APIs |
| Comparisons | **none** | Foundation UI |
| Tutorials | **none** | Foundation UI |

Save buttons on `none` workspaces show **«در نسخه آینده فعال می‌شود»** and do **not**
call APIs.

---

## Future backend extensions

- Dedicated Tool / Roadmap / Comparison / Tutorial aggregates or Content subtypes
- Persist roadmap steps/topics/resources
- Tool catalog fields + Toolbox binding
- Optional Content∪PromptLab bridging

Until then, UI foundations must stay honest about unsupported saves.

---

## Explicit non-goals

- Backend changes, migrations, new endpoints
- Fake persistence or invented fields stored as Content body hacks
