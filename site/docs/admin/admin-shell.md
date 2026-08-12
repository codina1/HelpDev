# Admin Shell v1

The Admin CMS runs in a **completely independent shell** mounted at `/admin`. It
does not reuse the marketing/user-panel chrome. This document describes the
architecture and its extension points.

## Route architecture

```
site/src/app/admin/
├── layout.tsx          # pre-paint theme script + <AdminShell>
├── page.tsx            # dashboard
├── loading.tsx         # content skeleton
├── error.tsx           # safe error boundary
├── not-found.tsx       # admin-scoped 404
├── content/ (+ new/)   # content module (placeholder)
├── learning/           # learning module (placeholder)
├── toolbox/            # toolbox module (placeholder)
├── prompt-lab/         # prompt lab module (placeholder)
├── users/              # users management (functional)
├── analytics/          # analytics (placeholder)
├── audit/              # audit (placeholder)
├── operations/         # health/operations (placeholder)
└── settings/           # settings (placeholder)
```

### User / Admin separation

`components/layout/app-shell.tsx` renders the marketing header/footer only for
public routes. Both `/profile` and `/admin` render their own full-page shells
while still sharing the single `AuthProvider`, so there is **one** auth context
and no duplicate profile requests. The user panel is otherwise untouched.

The legacy admin entry points are migrated:

- `/profile/admin` → redirects to `/admin`.
- The account menu "پنل ادمین" entry and `?tab=admin` now route to `/admin`.

## Component architecture

```
site/src/components/admin/
├── admin-preferences-provider.tsx   # theme + sidebar + group state (persisted)
├── admin-guard.tsx                  # loading/unauth/forbidden/allowed gate
├── layout/                          # shell, sidebar, header, drawer, content, footer
├── navigation/                      # nav-group, nav-item, breadcrumb
├── command/                         # command palette (+ provider), quick-create
├── feedback/                        # loading, error, empty, access-denied
├── page/                            # page-header, section, surface, stat-card, action-bar, module-placeholder
├── shared/                          # logo, environment-badge, theme-switcher, notifications, user-menu, menu, icons
└── views/                           # dashboard + users feature views
```

Supporting pure logic lives in `site/src/lib/admin/` (`routes`, `navigation`,
`permissions`, `route-matcher`, `breadcrumbs`, `command-menu`, `preferences`,
`environment`) and is fully unit-tested.

## Navigation config

All navigation is defined once in `lib/admin/navigation.ts` as typed
`AdminNavGroup[]`. Items reference **centralized routes** (`lib/admin/routes.ts`)
and icons **by name** (`AdminIconName`), keeping the config serializable and
testable. Items are either `ready` (linked) or `future` (rendered disabled with
a "به‌زودی" badge — no dead links, no fake screens).

## Permission model

`lib/admin/permissions.ts` maps a role to an `AdminPermission` set. Today the
backend exposes a single `Admin` role, which receives every permission; other
roles receive none. Navigation, command palette and quick-create are filtered by
this set (`filterAdminNavigation`, `buildCommandRegistry`). The structure is
granular so future backend roles can map to a subset **without inventing backend
permissions**.

> Frontend permission checks are **UX only**. Every Admin API call is authorized
> by the backend, which remains authoritative.

## Guard behavior

`AdminGuard` wraps the shell and derives access from the shared auth state via
`evaluateRouteAccess({ requireAdmin: true })`:

| State            | Behavior                                                        |
| ---------------- | -------------------------------------------------------------- |
| `loading`        | centered skeleton (prevents protected-page flash)              |
| `unauthenticated`| redirect to `/?next=<safe admin path>` (validated return URL)  |
| `forbidden`      | `AdminAccessDenied` page with a link back to the user panel    |
| `allowed`        | render the Admin shell                                         |

Expired sessions are cleared by the `AuthProvider` and surface as
`unauthenticated`.

## Sidebar persistence & theme (no flash)

Preferences are stored under the **versioned** key
`helpdev.admin.preferences.v1` (`sidebarCollapsed`, `theme`, `collapsedGroups`).
Malformed JSON falls back to defaults per field.

`app/admin/layout.tsx` injects a tiny **pre-paint inline script** that reads the
stored preference and sets `data-admin-theme` and `data-admin-sidebar` on
`<html>` before first paint. Sidebar width is a CSS variable driven by that
attribute, so there is **no hydration flash and no layout shift**. The
`AdminPreferencesProvider` then syncs React state and keeps the attributes and
storage up to date, and listens to the OS `prefers-color-scheme` for `system`.

## Responsive behavior

- **Desktop (≥1024px):** sticky header + collapsible sidebar (272px ↔ 76px).
- **Tablet/Mobile (<1024px):** sidebar becomes an off-canvas drawer opening from
  the inline-end; compact header; command/search always reachable.

## Command palette

`Ctrl/Cmd + K` (or the header search button) opens the palette. It searches the
permission-filtered navigation + quick-create commands with tolerant
Persian/English matching, supports full keyboard navigation (↑/↓/Enter/Esc), and
requires no network. It is intentionally scoped to navigation — global content
search is out of scope for Phase 1.

## Extension points

- **New module:** add a `ready` item in `navigation.ts` pointing at a new route
  in `routes.ts`, then add the `app/admin/<module>/page.tsx`. Breadcrumbs, active
  matching and the command palette pick it up automatically.
- **Granular permissions:** extend `AdminPermission` and `getPermissionsForRole`
  when the backend introduces new roles; navigation/commands filter themselves.
- **Notifications:** `admin-notifications-button.tsx` renders an empty state;
  wire it to a real feed and derive the unread badge from that response only.
- **Badges:** nav items accept an optional `badge` (`value` + `tone`).
