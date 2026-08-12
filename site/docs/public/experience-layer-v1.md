# Sprint 50C — HelpDev Experience Layer v1

Frontend experience layer that positions HelpDev as an **AI Engineering Knowledge Platform** (Linear × Vercel × Raycast × Notion × GitHub feel).

## API untouched confirmation

- No backend / migration / database / API contract changes
- Reuses: `listPublishedContent`, `listTools`, `search`, `searchAsk`, `fetchLearningProfile`, `fetchLearningRecommendations`, `fetchLearningRoadmap`
- No invented catalog numbers or fake recommendations

## Components — `src/components/experience/`

| Component | Role |
|-----------|------|
| `KnowledgeGalaxy` | Decorative animated knowledge graph |
| `InteractiveNode` | Hover/focus glow + description tooltip |
| `AIEntryExperience` / `AICommandBox` | Raycast-like entry |
| `CommandSearchBox` | Command field + suggestions |
| `PersonalizedHero` | Auth-only learning profile / recs / roadmap |
| `SmartEmptyState` | Premium empty messaging |
| `PremiumSectionHeader` | Section identity + CTA |
| `EngineeringTimeline` | Visual path nodes |
| `FeatureShowcase` | Hero capability grid |
| `HeroExperience` | New homepage hero composition |

## Pages updated

- `/` — HeroExperience, PersonalizedHero, upgraded sections
- Ctrl/Cmd+K — full-screen overlay with Knowledge / Tools / Roadmaps / AI sections

## UX improvements

- Galaxy left / copy right (mobile: headline → AI → galaxy)
- Premium empty states for roadmap / recommendations / tools / articles
- Section headers with icon, description, CTA
- Micro-interactions: float, edge dash, card lift, fade-up
- Personalized strip only when authenticated; empty when APIs return nothing

## Tests

- `experience-components.test.tsx`
- `knowledge-galaxy.test.tsx`
- `ai-entry.test.tsx`
- `empty-state.test.tsx`
- `responsive-layout.test.tsx`

## Limitations

- Galaxy is decorative (no functional node routing by design)
- Frontend Engineer timeline remains a structural UI demo, labeled as such
- PersonalizedHero depends on learning personalization APIs being available for the user
- Reading-time estimates on article cards remain soft UI helpers from Sprint 50B
