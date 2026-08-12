# Sprint 50D-2 — HelpDev Public Experience Redesign v1

Frontend-only homepage redesign into a premium **AI Engineering Platform** experience.

## Homepage sections

1. Hero — Engineering Intelligence + knowledge graph + CTAs  
2. Personalized strip (auth)  
3. Latest Articles — `ArticleCardPro`  
4. Engineering Tools — `ToolCardPro`  
5. Learning Roadmaps — timeline + `RoadmapCardPro`  
6. AI Assistant — `AiFeatureCard`  
7. Knowledge Search — search section + Ctrl+K  

## Pro components — `src/components/public/pro/`

| Component | Highlights |
|-----------|------------|
| `ArticleCardPro` | Cover, category, reading time, difficulty, tech tags |
| `ToolCardPro` | Icon, rating placeholder chrome, use cases, stack tags |
| `RoadmapCardPro` | Level, steps, structural progress track |
| `AiFeatureCard` | Premium AI CTA |

## Data rules

- No fake catalog content or invented ratings/scores  
- Tech tags only when keywords appear in title/slug  
- Tool rating is labeled «امتیاز به‌زودی»  
- Roadmap progress is a structural track, not fake %  

## Motion

`ds-slide`, `ds-fade`, `ds-glow`, hover lift via DS cards / galaxy animations  

## APIs reused

`listPublishedContent`, `listTools`, existing search / learning clients — unchanged  
