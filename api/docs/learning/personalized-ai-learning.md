# Personalized AI Learning Assistant v1

Suggestion-only learning personalization for HelpDev learners.

## Architecture

```
User UI  /learning/profile  +  /learning/assistant
        │
        ▼
LearningPersonalizationMeController  (/api/v1/me/...)
        │
        ▼
Learning.Application.Personalization
  ├─ LearningProfile / Preferences     (user-owned)
  ├─ LearningSignals                   (from enrollments/progress)
  ├─ ILearningRecommendationService    (deterministic items + AI explanation)
  ├─ ILearningRoadmapService           (AI suggests → user approves)
  └─ ILearningKnowledgeRetriever       (port)
                │
                ▼
Infrastructure adapter → Search IRagContextBuilder
                │
                ▼
IAiTextGenerator (Infrastructure.Ai) — generation only
```

| Module | Owns |
|--------|------|
| Learning | Profile, preferences, roadmap, signals, recommendation orchestration |
| Search/RAG | Knowledge retrieval |
| AI Infrastructure | Text generation |
| Analytics | `recommendation_requested` / `roadmap_generated` metrics |
| Identity | Auth only (no learning profile storage there) |

## Data sources

Signals use **existing** Learning data only:

- course enrollments
- lesson progress / completions
- content-linked lesson completions (`Lesson.ContentId`)

No invented likes, shares, or click scores.

Recommendations combine:

1. User profile + preferences
2. Learning signals
3. Published course catalog matches
4. HelpDev knowledge (RAG)
5. AI explanation (Reason / NextSteps) — **no ranking scores**

## Privacy model

Never stored in analytics/audit:

- private prompts
- AI response text
- full learning goals in admin APIs

Admin `GET /api/v1/admin/learning/personalization` returns **counts only**.

Audit/analytics metadata: `item_count`, `generation_type`.

## AI limitations / governance

AI **cannot**:

- enroll the user
- change lesson/course progress
- publish courses
- overwrite the learning profile

AI **only** suggests recommendations and roadmap steps. Roadmaps stay `Suggested` until the user calls `POST /me/roadmap/approve`.

## APIs

| Method | Path | Auth |
|--------|------|------|
| GET/PUT | `/api/v1/me/learning-profile` | Authenticated |
| GET | `/api/v1/me/recommendations` | Authenticated |
| GET | `/api/v1/me/roadmap` | Authenticated |
| POST | `/api/v1/me/roadmap/generate` | Authenticated |
| POST | `/api/v1/me/roadmap/approve` | Authenticated |
| GET | `/api/v1/admin/learning/personalization` | AdminOnly |

## Future roadmap

- Richer skill graph linked to PromptLab/Toolbox usage
- Per-user content interaction signals when a dedicated query exists
- Optional writer-curated learning paths as first-class catalog entities
