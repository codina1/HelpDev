# Sprint 50E — HelpDev Product Experience Layer

Frontend-only upgrade: homepage as an **AI engineering platform** experience.

## Homepage order

1. Hero — interactive knowledge graph (clickable nodes, AI glow, animated edges) + Ask HelpDev AI entry  
2. Personalized strip (auth)  
3. **Trust Metrics** — real counts from published catalog  
4. Intelligence Articles — `ArticleCardPro` + AI summary chrome  
5. Engineering Tools  
6. Learning Roadmaps — levels, lock/unlock, completion indicators  
7. AI Assistant  
8. **Ask HelpDev AI** — prompt interface (replaces plain keyword search chrome)  

## Upgrades

| Area | Change |
|------|--------|
| Knowledge Galaxy | Clickable orbit nodes → real routes; center opens palette; stronger glow + edge pulse |
| Search | Title `Ask HelpDev AI`; placeholder SaaS architecture prompt |
| Article cards | Category, difficulty, tags, reading time, **AI summary** (title/slug-derived insight) |
| Roadmaps | Levels, structural progress, locked/unlocked/current/completed steps |
| Trust Metrics | Engineering Articles, Learning Paths, Developer Tools, AI Guides |

## Data rules (unchanged)

- No backend / API / DB changes  
- No invented catalog totals or fake user progress %  
- AI summary is presentation chrome from title/slug keywords  
- Roadmap lock/completion is structural path preview  
- Trust metrics use `listPublishedContent` / `listTools` lengths + AI keyword filter  
- RTL + dark premium SaaS style preserved  

## APIs reused

`listPublishedContent`, `listTools`, existing search / learning clients — unchanged  
