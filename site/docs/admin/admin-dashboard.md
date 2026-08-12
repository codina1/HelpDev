# Admin Dashboard — HelpDev Command Center v1

The `/admin` route is the HelpDev **Admin Command Center**: a dense, read-only
operational overview built entirely on top of existing `/api/v1` endpoints. It
introduces **no backend changes, no migrations and no new API contracts**, and
never displays fabricated metrics — every number originates from a live API.

This document covers the dashboard architecture, the widget system, data
sources, extension points and future widgets.

---

## 1. Architecture

The dashboard follows a strict separation of concerns:

```
src/lib/admin/dashboard/        ← data layer (no JSX)
  dashboard-types.ts            ← raw DTOs (mirror backend JSON) + view models
  dashboard-api.ts              ← thin fetchers over the shared apiRequest client
  dashboard-mappers.ts          ← pure DTO → view-model mapping (unit-tested)
  dashboard-hooks.ts            ← useAdminDashboard() orchestrates parallel fetches

src/components/admin/dashboard/ ← presentation layer
  admin-dashboard.tsx           ← composition only (no business logic)
  widgets/                      ← reusable, prop-driven widgets
    widget-card.tsx             ← shared Loading/Error/Empty/Success shell
    status-badge.tsx            ← operational status badge + statusLabel()
    dashboard-header.tsx
    kpi-card.tsx / kpi-grid.tsx
    content-pipeline-card.tsx
    system-health-card.tsx
    operations-summary-card.tsx
    activity-feed-card.tsx
    quick-actions-card.tsx
    recent-content-card.tsx
    module-status-card.tsx
    dashboard-skeleton.tsx
    charts/
      dashboard-bar-chart.tsx   ← dependency-free SVG bar primitive
      user-growth-chart.tsx     ← lazy-loaded
      content-growth-chart.tsx  ← lazy-loaded
```

**Key rules enforced by this design**

- All API calls live in `lib/admin/dashboard` — components never call the network
  directly.
- Mapping logic is pure and unit-tested (`dashboard-mappers.test.ts`).
- Widgets are dumb and prop-driven: they receive an `AsyncSection<T>` and render
  the appropriate state. This makes them trivially testable and reusable.
- The page component (`admin-dashboard.tsx`) is composition only.

### Data flow

```
useAdminDashboard()  ──▶  dashboard-api (apiRequest / content module)
       │                        │
       │                        ▼
       │                   raw *Dto JSON
       │                        │
       │                   dashboard-mappers (pure)
       ▼                        ▼
AsyncSection<ViewModel> ──▶ widgets ──▶ WidgetCard state machine
```

`useAdminDashboard` fires all requests **in parallel** (never waterfalled),
aborts in-flight requests on unmount/reload, and tracks each section
independently so one failing endpoint never blanks the whole page. A single
`reload()` callback is passed to every widget's error state.

---

## 2. Widget system

Every data-backed widget is built on `WidgetCard`, which centralizes the four
UI states so behaviour is consistent everywhere:

| State    | Trigger                              | Rendering                          |
| -------- | ------------------------------------ | ---------------------------------- |
| Loading  | `loading`                            | shimmer skeleton (or custom node)  |
| Error    | `error` truthy                       | `AdminErrorState` (safe, + retry)  |
| Empty    | `isEmpty` and not loading/error      | `AdminEmptyState` (Persian copy)   |
| Success  | otherwise                            | `children`                         |

`AdminErrorState` shows a friendly Persian message plus the `correlationId` when
the error is an `ApiClientError`. It **never** renders stack traces, raw backend
payloads or technical details.

KPI tiles reuse the shared `AdminStatCard` primitive via `KpiCard`, which adds a
compact per-metric error state.

---

## 3. Data sources

All routes are canonical and versioned (`/api/v1`, resolved from the client base
URL). No new endpoints were added.

| Widget                    | Endpoint / source                                | Notes |
| ------------------------- | ------------------------------------------------ | ----- |
| KPI: Users / Content / Courses | `GET /admin/dashboard`                      | flat DTO: `users`, `content`, `learning` |
| KPI: System               | `GET /admin/operations/health`                   | overall + healthy component count |
| Content pipeline          | `GET /admin/dashboard` (`content`)               | Draft & Published only (see §5) |
| System health             | `GET /admin/operations/health`                   | 6 subsystems mapped to friendly labels |
| Operations summary        | `GET /admin/operations/status`                   | Outbox / Search / Analytics / Audit |
| Activity feed             | `GET /admin/audit?page=1&pageSize=8`             | actor + action + time only |
| Recent content            | `GET /content` (existing public content module)  | sorted newest-first, top 6 |
| User / Content charts     | derived from `/admin/dashboard` snapshot         | composition, not time-series |

### Health component mapping

`operations/health` returns components named `self`, `postgresql`,
`search_projection`, `outbox`, `analytics`, `audit`. These are mapped, in a fixed
order, to: **API, پایگاه داده (Database), جستجو (Search), Outbox, تحلیل‌ها
(Analytics), Audit**. Missing components render as `نامشخص` (Unknown). Statuses
are normalized to `Healthy` / `Degraded` / `Unhealthy` / `Unknown` and rendered
as semantic badges. Raw `safeDetails`, codes and durations are intentionally not
surfaced.

### Activity feed safety

Only `actor`, a friendly `action` label and relative `time` are shown. Audit
`metadata`, `correlationId`, request paths and payloads are never rendered. Known
audit actions map to Persian labels; unknown actions are safely humanized.

---

## 4. Layout & responsive behaviour

Sections, top to bottom:

1. **Header** — title «داشبورد مدیریت», description «مرکز کنترل HelpDev»,
   environment badge, actions (ایجاد محتوا / مدیریت سیستم).
2. **KPI overview** — 4 cards (Users, Content, Courses, System).
3. **Main operations** — Content pipeline | System health.
4. **Operations & modules** — Operations summary | Module status.
5. **Analytics** — User composition | Content composition (lazy charts).
6. **Activity & actions** — Activity feed | Quick actions.
7. **Recent content** — compact, responsive list.

Grid behaviour (Tailwind, logical properties):

| Breakpoint | KPI grid   | Widget rows |
| ---------- | ---------- | ----------- |
| Mobile (`<640px`)  | 1 column | 1 column |
| Tablet (`sm`, ≥640px) | 2 columns | 1–2 columns |
| Desktop (`lg`, ≥1024px) | 4 columns | 2 columns |

No widget uses fixed left/right positioning; spacing, alignment and the SVG bar
order all respect RTL. The recent-content list uses a wrapping grid instead of a
wide table, so there is **no horizontal scrolling** at 375px.

---

## 5. Content pipeline states

The backend currently exposes only **Draft** and **Published** counts (via
`/admin/dashboard`). The card shows exactly these. `Scheduled`, `Review`,
`Approval` and `SEO` are **not fabricated** — they are documented future states
(see §7) and will be added only when the backend exposes them.

---

## 6. Theming, RTL & performance

- **Theme**: all colors use `--adm-*` tokens keyed on `data-admin-theme`
  (light/dark). No hardcoded hex values in widgets. Icons inherit `currentColor`.
- **RTL**: logical CSS (`inline-start/end`, grid order); the SVG bar chart
  renders its first datum at the inline-start (right) edge.
- **Performance**:
  - Parallel fetch via `useAdminDashboard` (no request waterfalls).
  - In-flight requests aborted on unmount/reload; no duplicate requests.
  - Charts are `next/dynamic` client-only imports (own chunks) with skeleton
    fallbacks, keeping the initial dashboard payload small.
  - Mapping is pure and cheap; view models are computed once per fetch.

---

## 7. Extension points & future widgets

**Add a new widget**

1. Add the DTO + view-model types to `dashboard-types.ts`.
2. Add a fetcher to `dashboard-api.ts` (reuse `apiRequest`, canonical route).
3. Add a pure mapper to `dashboard-mappers.ts` and cover it in tests.
4. Wire a new `AsyncSection` into `useAdminDashboard`.
5. Build a prop-driven widget on top of `WidgetCard` and place it in
   `admin-dashboard.tsx`.

**Future widgets (require backend support first)**

- **True growth charts** (user/content over time) — needs a time-series endpoint;
  the current charts are single-snapshot composition views.
- **Extended content lifecycle** — `Scheduled`, `Review`, `Approval`, `SEO`
  states once the content module exposes them.
- **Real-time notifications** feed.
- **Per-actor activity drill-down** with server-side filtering.

---

## 8. Testing

- `dashboard-mappers.test.ts` — pure mapping, status normalization, activity
  safety (no metadata leak), recent-content sorting/limit, relative-time and
  number formatting.
- `dashboard-api.test.ts` — canonical/versioned endpoints only, no client
  duplication, and a guard asserting **no sample/fabricated metrics** appear as
  literals in dashboard sources.
- `dashboard-widgets.test.tsx` — KPI rendering and the Loading/Error/Empty/
  Success state machine for the shared widgets (rendered via
  `react-dom/server`), verifying errors never leak raw payloads.
