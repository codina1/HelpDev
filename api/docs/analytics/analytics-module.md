# Analytics module

HelpDev Analytics is an event-driven aggregation module. Metrics land in
`analytics_daily_metrics` via idempotent ingestion (`analytics_event_receipts`).

## Storage (existing)

| Table | Role |
| --- | --- |
| `analytics_daily_metrics` | Daily counters / duration aggregates |
| `analytics_event_receipts` | Idempotency by `EventId` |
| `analytics_daily_active_users` | DAU markers |
| `analytics_subject_snapshots` | Display names for top-N |

Sprint 37 **does not** add `content_analytics_metrics`. Content analytics reads the
existing daily metrics with `SubjectId` filters to avoid double-counting global +
subject view rows.

## AI usage (Sprint 38)

Table `ai_usage_records` stores token/provider telemetry for Content AI tasks.
It never stores prompts, generated text, or secrets. See
[`../ai/ai-content-assistant.md`](../ai/ai-content-assistant.md).

## Content Analytics Platform (v1)

### Domain

- `ContentMetricType` — View (supported); SearchClick / Favorite / Save / Share /
  Completion reserved.
- `ContentAnalyticsSnapshot` — immutable analytical snapshot (no update/delete).
- `ContentHealthEvaluator` — transparent status + reasons (no fake score).

### Queries

`IContentAnalyticsQueries`:

- `GetContentOverviewAsync`
- `GetTopContentAsync`
- `GetContentPerformanceAsync`
- `GetContentHealthAsync` / `GetContentHealthByIdAsync`

Content editorial facts (SEO gaps, revisions, `UpdatedAt`) come from
`IContentAnalyticsFactsSource` implemented in host Infrastructure
(`ContentAnalyticsFactsSource`) so Analytics Application does not reference
Content Infrastructure.

### Events

Existing pipeline continues:

- `content.item_viewed` (direct ingest on published slug read)
- `content.item_created` (direct ingest)
- `content.item_published` (Outbox → `ContentPublishedAnalyticsHandler`)

**Not added:** `ContentViewedDomainEvent` / `ContentFavoritedDomainEvent` — views
already use analytics envelopes; favorites for Content do not exist.

### Admin HTTP

See `AnalyticsAdminController`:

- `GET /api/v1/admin/analytics/content`
- `GET /api/v1/admin/analytics/content/{id}`
- `GET /api/v1/admin/analytics/top-content`
- `GET /api/v1/admin/analytics/content-health`

Plus legacy overview / time-series / top / search / toolbox / prompt-lab routes.

### Limitations

- No Google Analytics / Matomo / AI predictions / traffic estimation.
- Entity `Content.Views` / `Content.Saves` columns are **not** the analytics source of truth
  (entity counters are not incremented by the app).
- Favorite / Share / Save / Completion metrics are not produced for Content.
- Overview KPI double-counting on some shared metric keys (global + subject) remains in
  the legacy overview path; content analytics overview uses `SubjectId == null` for
  global totals.

### Future extension points

- Emit `search.document_indexed` when Search indexing completes.
- Content favorite / save producers + metric keys if product adds those actions.
- Persist SEO analysis history for richer health (today SEO health uses stored metadata gaps).
