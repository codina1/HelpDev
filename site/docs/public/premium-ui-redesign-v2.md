# Sprint 50B — HelpDev Premium UI Redesign v2

Frontend experience redesign of the public product surface into an **AI Engineering Knowledge Platform**.

## Scope

- Frontend only — no backend, migrations, API, or business-logic changes
- Reuses App Router, auth, search, content, and toolbox clients

## Design tokens

Added under `:root` in `globals.css`:

| Token | Role |
|-------|------|
| `--pub-bg` / `--pub-bg-elevated` | Deep developer dark |
| `--pub-glass` / `--pub-glass-strong` | Glass surfaces |
| `--pub-primary` / `--pub-primary-2` | Purple / Indigo |
| `--pub-secondary` | Cyan |
| `--pub-ai-from/via/to` | AI gradient |
| `--pub-glow` / `--pub-shadow*` | Glow + elevation |

Utilities: `.pub-glass`, `.pub-border-gradient`, `.pub-card-elevate`, `.pub-fade-up`, `.pub-gradient-shift`.

## Components — `src/components/ui/public/v2/`

`PublicContainer`, `PublicSection`, `GlassCard`, `GradientText`, `GlowButton`, `PremiumBadge`, `KnowledgeCard`, `ToolCard`, `RoadmapCard`, `AICommandBox`, `FeatureGrid`, `AnimatedBackground`

## Routes / screens redesigned

| Route | Change |
|-------|--------|
| `/` | Homepage v2 — hero, AI command, knowledge, tools, roadmap timeline, AI CTA |
| `/articles` | Featured hero + premium grid + filters |
| `/articles/[slug]` | Glass reading layout + TOC + AI panel |
| Ctrl/Cmd+K | Raycast-style command palette + AI answer (`searchAsk`) |

## Chrome

- Sticky glass header — Logo, Products, AI Search, Dashboard, Avatar
- Mobile bottom nav — Home / Search / Learning / Profile

## APIs reused

- `listPublishedContent`, `getContentBySlug`
- `listTools`
- `search`, `searchAsk`

## Tests

- `public-design-system.test.tsx`
- `homepage-render.test.tsx`
- `search-ui.test.tsx`
- `responsive-component.test.tsx`

## Limitations

- Reading time / difficulty on cards are soft UI estimates when public DTOs lack rich metadata
- Roadmap “Frontend Engineer” path is a **structural visual demo**, not fake catalog content
- Related knowledge remains a placeholder
- No framer-motion — CSS animation utilities only
- Light mode: tokens are ready; public shell stays dark-first
