# Product Readiness v1 — Sprint 45

**Sprint:** 45 — Product Completion Audit v1  
**Related audit:** [product-readiness-audit-v1.md](./product-readiness-audit-v1.md)

## Completed areas

| Area | Delivery |
|------|----------|
| Product audit | Flow gaps documented |
| `/dashboard` | Welcome, enrollments progress, continue learning, AI recommendations, roadmap, recent enrollments, quick links — **real APIs only** |
| `/learning` | Enrolled courses, roadmap, recommendations, recent enrollments |
| Learning cards | `CourseCard`, `ProgressCard`, `RecommendationCard`, `RoadmapCard` |
| Search | Ctrl+K `GlobalSearchPalette` + `/search` via `GET /search` (content/course/lesson/tool/prompt filters) |
| Notifications | `NotificationCenter` empty/read-unread foundation — **no fake items** |
| `/settings` | Profile + learning preferences (live APIs); AI + security show **unavailable** (no fake save) |
| Admin ops | Expanded dashboard quick actions (content, workflows, media, SEO, operations) |
| CMS polish | Row selection + bulk toolbar foundation; status legend Draft→Archived; disabled bulk actions explain why |
| Shared states | `PageEmptyState`, `PageErrorState`, `PageLoadingState` (RTL, correlation id) |
| Nav | Header links for یادگیری / داشبورد; notification bell for signed-in users |

## Tests

- `site/src/lib/product-readiness.test.ts`
- `site/src/lib/product-ux.test.tsx`

## Remaining limitations

- No user notifications backend — center stays empty until an API exists.
- No AI preference / security session APIs — settings sections are explicitly unavailable.
- Bulk publish/archive not supported by API — UI selects rows but actions stay disabled.
- Public `/courses` catalog may still show legacy static cards; learning hub uses enrollments + `GET /learning/courses`.
- Enrollment list items do not include lesson titles; “recent lessons” is approximated via recent enrollments/progress until a richer DTO exists.
- PostgreSQL E2E / live search quality depends on indexed published content.

## Phase 2 candidates

1. Wire public courses page to `listCourses` end-to-end with enroll CTA.
2. Notifications feed + unread badge from backend.
3. Supported bulk CMS endpoints (or remove selection when permanently unsupported).
4. Lesson-level “continue” deep links from enrollment detail.
5. User security center (active sessions) when Identity APIs allow.
6. Accessibility audit pass with automated axe checks in CI.

## Verification

```bash
dotnet build -c Release
cd site && npm run lint && npm run typecheck && npm test && npm run build
```
