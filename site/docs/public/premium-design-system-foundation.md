# Sprint 50D-1 — HelpDev Premium Design System Foundation

Unified premium SaaS design system for HelpDev (frontend only).

## Theme

| Role | Value |
|------|--------|
| Background | `#060816` |
| Primary | `#8B5CF6` |
| Secondary | `#06B6D4` |

Tokens live in CSS (`--ds-*`) and TS (`src/lib/design-system/tokens.ts`). Legacy `--background` / `--pub-*` alias into DS tokens so existing public UI picks up the theme.

## Primitives — `src/components/ui/ds/`

Button, Card, Badge, Input, Tabs, Modal, EmptyState, LoadingState, ErrorState

## Card variants

ArticleCard, ToolCard, RoadmapCard, AiCard

## Animations

`.ds-hover-lift`, `.ds-glow`, `.ds-fade`, `.ds-slide` (+ reduced-motion guards)

## Showcase

`/design-system` — colors, typography, buttons, cards, badges, states

## Refactors

- `PageEmptyState` / `PageLoadingState` / `PageErrorState` → DS tokens
- Legacy `Card` → DS surface utilities
- `GlowButton` → wraps DS `Button`

## Constraints honored

No backend / API / DB changes. Routes preserved. RTL kept.
