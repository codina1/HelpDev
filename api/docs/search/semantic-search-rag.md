# Semantic Search + RAG Platform v1

Sprint 39 adds knowledge retrieval over published HelpDev content using
**pgvector**, Outbox-driven indexing, and grounded RAG answers.

## Architecture

```
Content Published / Updated
        │  Outbox domain events
        ▼
Search handlers
  ├─ lexical SearchDocument projection (existing)
  └─ SemanticIndexingService
        ├─ MarkdownContentChunker (deterministic, no AI)
        ├─ IEmbeddingGenerator (Fake | Http)
        └─ search_chunks + search_vectors (pgvector)
        ▼
GET /api/v1/search/semantic   → ISemanticSearchQueries
POST /api/v1/search/ask       → IRagAnswerService → IAiTextGenerator
```

### Boundaries

| Module | Owns | Must not |
| --- | --- | --- |
| Content | Source content + events | Search Infra, vectors, AI providers |
| Search | Chunks, vectors, retrieval, RAG orchestration | Provider SDKs |
| Infrastructure.Ai | `IEmbeddingGenerator` / `IAiTextGenerator` adapters | Controllers / Content Domain |
| Controllers | HTTP + DTOs | DbContext, raw vectors |

## Database

Requires **pgvector** (`CREATE EXTENSION vector`).

Dev/test images:

- `docker-compose.yml` → `pgvector/pgvector:pg16`
- Testcontainers → `pgvector/pgvector:pg16`

Tables (migration `AddSemanticSearchRagV1`):

| Table | Purpose |
| --- | --- |
| `search_chunks` | Immutable text chunks |
| `search_vectors` | `vector(384)` embeddings + HNSW cosine index |
| `search_semantic_index_states` | Indexed/Failed status for admin UI |

## APIs

| Method | Path | Auth |
| --- | --- | --- |
| GET | `/api/v1/search/semantic?q=` | Anonymous + rate limited |
| POST | `/api/v1/search/ask` | Anonymous + rate limited |
| GET | `/api/v1/search/manage/knowledge` | AdminOnly |
| GET | `/api/v1/search/manage/related?sourceType&sourceId` | AdminOnly |

DTOs never expose embeddings, prompts, or API keys.

## Admin UI

- `/admin/search/knowledge` — documents/chunks/status/failures
- Content Studio **Related Knowledge** panel — suggestions only (no auto-link)

## Security

- Never log prompts, chunk bodies, embeddings, or API keys
- Audit `search.rag_query_requested` with metadata `{ taskType }` only
- Answers are grounded in retrieved HelpDev snippets (no web search)

## Configuration

```json
"Embedding": {
  "Enabled": true,
  "ProviderName": "Fake",
  "Model": "fake-embed-v1",
  "Dimensions": 384
}
```

`Fake` is for tests/dev (deterministic hashing). Production uses `Http` against a
configured embedding endpoint. Similarity is always computed by **real pgvector**
cosine distance — never fabricated scores.

## Limitations

- Content sources only for semantic indexing in v1 (courses stay lexical)
- No ChatGPT-style chat UI / autonomous agents
- No external crawling / web search
- No automatic content rewriting or linking
- Fake embeddings are not a production model
- Dimensions fixed at 384 for schema stability

## Future

- Course/toolbox/promptlab semantic sources
- PromptLab-backed RAG instructions
- Hybrid lexical + semantic ranking
- Admin reindex that also rebuilds semantic chunks
