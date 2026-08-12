# Sprint 50F — HelpDev Intelligence Showcase

Frontend-only completion of the homepage premium experience.

## New sections

1. **Engineering Intelligence** — HelpDev philosophy + AI engineering visual  
2. **AI Workflow Demo** — animated pipeline: Question → Understanding → Analysis → Roadmap → Solution  
3. **Developer Journey** — Beginner → Developer → AI Engineer → Architect  
4. **Engineering Case Studies** — methodology patterns + optional published articles  

## Homepage order

Hero → Personalized → Trust Metrics → **Intelligence** → **Workflow** → Articles → Tools → Roadmaps → **Journey** → **Case Studies** → AI Assistant → Ask HelpDev AI  

## Style

Dark premium AI platform, RTL, glass cards, subtle CSS motion (Vercel/OpenAI-inspired), `prefers-reduced-motion` respected.

## Data rules

- No backend changes  
- Case patterns are product methodology chrome (not fake catalog articles)  
- Published examples only when `listPublishedContent` returns real items  
- Reuses existing DS / public v2 primitives  

## Key files

- `src/lib/public/intelligence-showcase.ts`  
- `src/components/public/home/v2/engineering-intelligence-section.tsx`  
- `src/components/public/home/v2/ai-workflow-demo.tsx`  
- `src/components/public/home/v2/developer-journey-timeline.tsx`  
- `src/components/public/home/v2/engineering-case-studies.tsx`  
