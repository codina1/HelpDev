# Roadmap Engine v1

**Sprint:** 49  
**Scope:** Content Core + Roadmap Metadata + Steps + Topics + Resources.

## Architecture

```
Content (lifecycle / SEO / search / revision / workflow)
  └── RoadmapMetadata (1:1)
        └── RoadmapStep (1:N phases)
              ├── RoadmapTopic (1:N)
              └── RoadmapResource (1:N)
```

- Content owns publish and indexing.
- Roadmap satellite stores level, duration, goal, prerequisites, and ordered phases.
- Resources reference Content / Tools / Learning by **identifier string only** (no hard FK).
- Distinct from Learning module `LearningRoadmap` (personalization).

## Domain

| Entity | Namespace |
|--------|-----------|
| `RoadmapMetadata` | `Domain.Roadmaps` |
| `RoadmapStep` | `Domain.Roadmaps` |
| `RoadmapTopic` | `Domain.Roadmaps` |
| `RoadmapResource` | `Domain.Roadmaps` |

Enums: `RoadmapLevel`, `RoadmapResourceType`.  
New `ContentType.Roadmap` for parent content.

## Persistence

Migration: `AddRoadmapEngineV1` (count: **24**)

| Table | Indexes |
|-------|---------|
| `roadmap_metadata` | unique `content_id` |
| `roadmap_steps` | `roadmap_id`, `(roadmap_id, sort_order)` |
| `roadmap_topics` | `step_id` |
| `roadmap_resources` | `step_id` |

## Application

- `IRoadmapService` — Create, Update, AddStep, UpdateStep, RemoveStep, ReorderSteps
- `IRoadmapQueries` — ownership-scoped list
- AI: `IRoadmapAiAssistantService` — outline / phases / topics (human apply only)
- Progress foundation: `IUserRoadmapProgressFoundation` (marker only — not implemented)

## API (WriterOrAdmin + ownership)

| Method | Route |
|--------|-------|
| GET/PUT | `/api/v1/admin/content/{id}/roadmap` |
| POST | `/api/v1/admin/content/{id}/roadmap/steps` |
| PUT/DELETE | `/api/v1/admin/content/{id}/roadmap/steps/{stepId}` |
| POST | `/api/v1/admin/content/{id}/roadmap/steps/reorder` |
| POST | `.../roadmap/ai/outline|phases|topics` |

## Frontend

- `/admin/content/roadmaps` — list
- `/admin/content/roadmaps/new` — create Content + metadata
- `/admin/content/roadmaps/[id]` — RoadmapBuilder (meta, steps, topics, resources, reorder, AI)

## SEO

- `IStructuredDataGenerator.GenerateLearningRoadmap` → schema.org `Course` foundation (no public injection).

## Limitations

- No user progress / enrollment
- No public roadmap page
- Reorder UI uses up/down controls (not HTML5 drag library)
- Resource picker uses free-text identifiers (no cross-module picker UI yet)
- Legacy `ContentType.RoadmapStep` remains for older content; new roadmaps use `Roadmap`
