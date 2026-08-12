# Sprint 50 — HelpDev Public Product UI/UX Foundation v1

Transforms the HelpDev public website into a premium **AI Engineering Knowledge Platform** surface.

## Scope

- Frontend only
- No backend / database changes
- Reuses existing public APIs
- Preserves App Router, RTL (`lang=fa dir=rtl`), and existing auth (OTP + `AuthProvider`)

## Routes

| Route | Description |
|-------|-------------|
| `/` | Redesigned homepage (Hero, AI Search, Latest Content, Tool Showcase, Roadmap Showcase, AI CTA, Footer) |
| `/articles` | Article listing with cards, filters, search, client pagination foundation |
| `/articles/[slug]` | Premium reading layout — TOC, metadata, related placeholder, AI assistant panel |
| `/search` | Global search page (enhanced labels + result links) |
| Ctrl/Cmd+K | Command palette (articles / tools / courses / prompts) |

## Design system — `src/components/ui/public/`

| Component | Role |
|-----------|------|
| `Container` | Horizontal gutter / max-width |
| `Section` | Vertical rhythm + optional container |
| `ContentCard` | Published content card |
| `ToolCard` | Tool showcase card |
| `RoadmapCard` | Roadmap showcase card |
| `Badge` | Token-based public badge |
| `GradientHeading` | Premium section / hero titles |
| `SearchBox` | Accessible search field |

## Chrome

- `PublicHeader` — RTL nav, mobile drawer, Ctrl+K, user menu / AuthModal
- `PublicFooter` — global marketing footer on public shells

## APIs reused

| API | Usage |
|-----|--------|
| `GET /content` (`listPublishedContent`) | Homepage latest + roadmaps/tools fallback; `/articles` |
| `GET /content/{slug}` (`getContentBySlug`) | `/articles/[slug]` |
| `GET /tools` (`listTools`) | Homepage tool showcase |
| `GET /search` (`search`) | Command palette + `/search` |
| `POST /search/ask` (`searchAsk`) | Article AI assistant panel |

## Theme & a11y

- Components use CSS variables (`--background`, `--accent`, …) for dark/light compatibility
- Public site remains dark by default (`html.dark`); tokens are theme-ready
- Keyboard: Ctrl/Cmd+K, Escape, arrow navigation in palette, focus-ring utilities
- ARIA: dialogs, listbox/option, pressed filters, search roles, nav labels

## Responsive

Layouts target **375 / 768 / 1024 / 1440** via Tailwind breakpoints (`sm` / `md` / `lg`).

## Tests

- `src/lib/public/public-ui.test.tsx` — design system render contracts + navigation helpers

## Limitations

- Article pagination is **client-side** over the published content list (no server page API yet)
- Related content on article detail is a **placeholder** (Knowledge related API is admin-scoped)
- Public roadmap/tool detail UIs beyond reserved routes remain thin foundations
- Light theme switcher is not shipped on the public shell (tokens only)
- Homepage/sections degrade to empty states when APIs are unreachable (no fake data)
- Markdown rendering is a lightweight heading/paragraph foundation (not a full MD engine)
