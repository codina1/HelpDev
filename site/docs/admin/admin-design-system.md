# Admin Design System v1

A dense, professional enterprise-CMS style, RTL-first, with independent
light/dark theming.

## Theming

The Admin area uses its **own token set** keyed on the `data-admin-theme`
attribute (`dark` default, `light` override). This is deliberately separate from
the global `.dark` class so Admin can switch themes without conflicting with, or
altering, the user-panel appearance. Tokens are defined in
`src/app/globals.css` under the "Admin CMS shell" section and consumed through
`.adm-*` utility classes and `var(--adm-*)` values.

Theme options: **light / dark / system**. `system` follows
`prefers-color-scheme` and updates live.

## Color semantics

| Role      | Token(s)                                   | Usage                     |
| --------- | ------------------------------------------ | ------------------------- |
| Primary   | `--adm-accent`, `--adm-accent-soft`        | indigo/purple accents     |
| Success   | `--adm-success`, `--adm-success-soft`      | healthy status, emerald   |
| Warning   | `--adm-warning`, `--adm-warning-soft`      | staging / pending, amber  |
| Danger    | `--adm-danger`, `--adm-danger-soft`        | errors / production, rose |
| Info      | `--adm-info`, `--adm-info-soft`            | neutral highlights, blue  |
| Neutral   | `--adm-surface*`, `--adm-text*`, `--adm-border*` | slate/zinc surfaces |

Status is never conveyed by color alone (icons + text labels accompany tone).

## Surfaces, borders, radius, density

- **Surfaces:** `--adm-surface` (cards), `--adm-surface-2` (hover),
  `--adm-surface-3` (chips/wells), `--adm-bg` (app background).
- **Borders:** subtle `--adm-border`, stronger `--adm-border-strong`.
- **Radius:** 10–14px (`rounded-lg`/`rounded-xl`); low-noise, not consumer-round.
- **Density:** compact typography (12–14px body), tabular numerics for metrics.

## Spacing

Content padding: mobile 16px, tablet 20–24px, desktop 24–32px. Header height
64px. Sidebar 272px expanded / 76px collapsed.

## Typography

Vazirmatn (inherited). Page titles ~20px bold; section titles ~15px bold; body
~13px; meta/subtle ~11–12px.

## Page primitives (`components/admin/page`)

- **AdminPageHeader** — `title`, `description`, `breadcrumbs?`, `primaryAction?`,
  `secondaryActions?`, `badge?`, `meta?`.
- **AdminSurface** — bordered surface with `padding` and `density` options.
- **AdminPageSection** — titled section with optional actions.
- **AdminStatCard** — `label`, `value`, `icon?`, `tone?`, `trend?`,
  `description?`, `loading?`.
- **AdminActionBar** — filters slot, actions slot, selection count, optional
  sticky.
- **AdminModulePlaceholder** — professional "در حال توسعه" module scaffold.

## Feedback states (`components/admin/feedback`)

- **AdminLoadingState** — stat/table skeletons; the sidebar/header stay stable
  (no full-page spinner after auth resolves).
- **AdminErrorState** — safe Persian message + correlation id (from
  `ApiClientError`) + retry / back-to-dashboard. Never shows stack traces or raw
  payloads.
- **AdminEmptyState** — icon + title + description + optional actions.
- **AdminAccessDenied** — forbidden page (user-panel link + logout).

## Icons

A single tree-shakable `AdminIcon` component (`shared/admin-icons.tsx`) renders
inline stroke SVGs inheriting `currentColor`, so icons adapt to both themes. No
external icon dependency is added; navigation icons are ~18px.

## Accessibility

- Semantic `nav`/`main`/`header` landmarks; skip-to-content link.
- `aria-current="page"` on the active nav item and current breadcrumb.
- Icon-only controls have `aria-label`s.
- `.adm-focus` provides visible focus rings; Escape closes drawer/menus/palette.
- Touch targets ≥40px; RTL-first layout.
