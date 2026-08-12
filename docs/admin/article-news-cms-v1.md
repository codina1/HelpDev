# Article & News Production CMS v1

**Sprint:** 47B  
**Scope:** ArticleMetadata + NewsMetadata extensions on Content Core (modular monolith).

## Architecture

```
Content (lifecycle)
  ├── SeoMetadata (owned VO / existing)
  ├── Media / Revision / Workflow / Analytics (existing modules)
  ├── ArticleMetadata (1:1 satellite table)
  └── NewsMetadata (1:1 satellite table)
```

- Content owns publish/lifecycle.
- Article/News metadata store **only** type-specific fields.
- No mega-Content table, no SEO/Media/Revision duplication.
- Search documents stay Content-event driven; metadata does **not** leak into search schema.

## Domain

| Entity | Namespace | Notes |
|--------|-----------|-------|
| `ArticleMetadata` | `Domain.Articles` | CategoryId?, Difficulty, ReadingTime, Featured, Comments, TOC |
| `NewsMetadata` | `Domain.News` | Source, SourceUrl, NewsDate, Priority, ExternalReference |
| `ContentCategory` | `Domain.Categories` | Extension point only — **no** DbSet/table in v1 |

## Persistence

Migration: `AddArticleNewsMetadataV1` (count: **22**)

| Table | Unique | FK |
|-------|--------|-----|
| `article_metadata` | `content_id` | → `contents` CASCADE |
| `news_metadata` | `content_id` | → `contents` CASCADE |

`contents` table unchanged.

## API (WriterOrAdmin + ownership)

| Method | Route |
|--------|-------|
| GET | `/api/v1/admin/content/{id}/article` |
| PUT | `/api/v1/admin/content/{id}/article` (upsert) |
| GET | `/api/v1/admin/content/{id}/news` |
| PUT | `/api/v1/admin/content/{id}/news` (upsert) |

Writer → own content only (cross-owner → `content_not_found`). Admin → all.

## Frontend

- `/admin/content/articles/[id]` → Content Studio + Article Settings panel
- `/admin/content/news/new` → Source / URL / Priority / Date (saved after create via news metadata API)
- `/admin/content/news/[id]` → Studio + News settings
- SEO remains existing `PUT .../seo`

## Limitations

- No full taxonomy engine (`ContentCategory` is a foundation type only)
- CategoryId is opaque GUID until catalog exists
- Article/News metadata are optional until first PUT
- Search indexing unchanged
