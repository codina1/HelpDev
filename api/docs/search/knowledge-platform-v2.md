# HelpDev Knowledge Platform v2

Unified semantic knowledge index across Content, Learning, Toolbox, and PromptLab.

## Architecture

```
Content / Learning / Toolbox / PromptLab
              │
         Domain Events (Outbox)
              │
              ▼
         Search Module
    (chunk → embed → search_vectors)
              │
              ▼
     Semantic Search / RAG
```

**Boundaries**

| Owns | Module |
|------|--------|
| Chunking, embeddings, vectors, retrieval | Search |
| Text generation | AI (`IAiTextGenerator`) |
| Source domain data | Content / Learning / Toolbox / PromptLab |

Source modules must not reference Search infrastructure, pgvector, or embedding providers.  
Search adapters live in `HelpDev.Infrastructure.Search` and read via module persistence ports only.

## Knowledge sources

| `KnowledgeSourceType` | Wire value | Body used for chunks |
|-----------------------|------------|----------------------|
| Content | `content` | Article body |
| Course | `course` | Course description |
| Lesson | `lesson` | Course title + lesson title + linked published Content (if any) |
| Tool | `tool` | Summary + description |
| Prompt | `prompt` | Summary + description + published template |

Only **published / enabled** entities are indexed. Drafts are never stored in the semantic index.

## Data flow

1. Source module raises a domain event (`*Published` / `*Updated` / `*Unpublished`).
2. Outbox persists the event; processor dispatches handlers.
3. Search semantic handler loads a `SearchSourceDocument` via the knowledge source adapter.
4. `MarkdownKnowledgeChunker` splits text deterministically.
5. `IEmbeddingGenerator` produces vectors (real provider or Fake for tests — no random similarity).
6. Rows upsert into `search_chunks` + `search_vectors`; state tracked in `search_semantic_index_states`.
7. Idempotency: same `LastEventId` + Indexed status skips rework.

### Events

| Event | Outbox type |
|-------|-------------|
| ContentPublished / Updated | `content.published.v1` / `content.updated.v1` |
| CoursePublished / Updated | `learning.course-published.v1` / `learning.course-updated.v1` |
| LessonPublished | `learning.lesson-published.v1` |
| ToolPublished / Unpublished | `toolbox.tool-published.v1` / `toolbox.tool-unpublished.v1` |
| PromptPublished / Unpublished | `promptlab.prompt-published.v1` / `promptlab.prompt-unpublished.v1` |

## APIs

### `GET /api/v1/search/semantic?q=`

Returns `SemanticSearchResponseDto`:

- `title`, `type`, `snippet`, `url`, `similarity`

Never returns vectors, embeddings, or internal chunk IDs.

### `POST /api/v1/search/ask`

RAG: retrieve all knowledge sources → `IRagContextBuilder` → `IAiTextGenerator`.

Response: `answer` + `sources` (metadata only).

### Admin

- `GET /api/v1/search/manage/knowledge?sourceType=` — filters: all / content / course / lesson / tool / prompt  
- `GET /api/v1/search/manage/related?sourceType=&sourceId=` — Related Knowledge for Content Studio (suggestions only; no auto-link)

## Security & audit

**Never stored:** user questions, AI answers, private drafts, API keys, raw embedding vectors in API payloads.

**Audit actions (metadata = source count only):**

- `search.semantic_search_requested`
- `search.rag_answer_requested`

## Schema

Sprint 39 tables reused (`search_chunks`, `search_vectors`, `search_semantic_index_states`).  
`source_type` already discriminates knowledge kinds — **no new migration** for v2 type expansion.

## Limitations

- Lessons without linked Content index title/course context only.
- Lexical reindex (`/search/manage/reindex`) still covers Content + Course documents only; semantic coverage is event-driven.
- No chatbot UI, crawling, or automatic content modification.
- Embedding quality depends on configured provider; Fake generator is deterministic for tests only.

## Future roadmap

- Semantic backfill job for historical published sources
- Lesson-native body field (if product adds lesson markdown)
- Optional per-type retrieval weights (explicit product decision)
- Admin semantic reindex by source type
