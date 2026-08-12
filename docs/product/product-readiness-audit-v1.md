# Product Readiness Audit v1 — Sprint 45

**Sprint:** 45 — Product Completion Audit v1  
**Goal:** Make the existing HelpDev platform usable as a real production product.  
**Non-goals:** Large features, new AI capabilities, architecture redesign, fake data, unsupported APIs.

## Method

Inspected frontend routes under `site/src/app`, public and Admin UI shells, API clients under `site/src/lib/api`, and matching backend controllers. Flows below reflect **pre-Sprint-45** state; remediations are tracked in [product-readiness-v1.md](./product-readiness-v1.md).

---

## Anonymous journey

| Step | Expected | Pre-sprint finding |
|------|----------|-------------------|
| Landing | Brand + explore | `/` exists; strong marketing surface |
| Register | OTP register/login | Auth modal on header; OTP APIs work |
| Explore content | Articles/news/courses | Public pages exist; courses UI used **static** data, not `GET /learning/courses` |
| Search | Find content | `/search` used **local static index**, not `GET /search` |
| Course discovery | Browse published courses | `/courses` static; learning catalog API unused |

**Broken / confusing**
- Ctrl+K only focused the header input; no command-palette results from Search API.
- Anonymous users could not see a clear path from search results to authenticated learning.

**Missing states**
- API-backed empty / error / loading on public search.

---

## Authenticated user journey

| Step | Expected | Pre-sprint finding |
|------|----------|-------------------|
| Login | OTP → JWT | Works via AuthModal |
| Profile | View/edit profile | `/profile` works with `GET/PUT /profile/me` |
| Learning dashboard | Progress + continue | **No `/dashboard`**; account home mixed **mock** cards |
| Course usage | Enrollments + progress | Enrollment APIs exist; **no frontend client** |
| AI assistant | Recommendations + roadmap | `/learning/assistant` + `/learning/profile` work |

**Broken / confusing**
- “Dashboard” lived inside `/profile` with mock learning/activity stats.
- No `/learning` hub — only profile + assistant.
- No `/settings` route (settings tab under profile only).
- No user notification center (admin bell stub only).

**Missing states**
- Shared PageEmpty / PageError / PageLoading for public product pages.

---

## Admin journey

| Step | Expected | Pre-sprint finding |
|------|----------|-------------------|
| Login | Admin guard | Works |
| Dashboard | Ops overview | `/admin` + real dashboard API |
| Content CMS | List / edit / workflow | Strong; status badges exist |
| Workflow | Submit / approve / publish | Exists |
| Analytics | Content analytics | Exists |
| System operations | Ops / audit / outbox | Exists |

**UX gaps (not broken APIs)**
- Quick actions omitted common daily links (workflows, media, SEO, operations).
- Content list had no selection column / bulk toolbar foundation.
- Empty/error patterns were Admin-only (`AdminEmptyState` etc.), not shared with public product.

---

## Cross-cutting gaps

| Area | Gap |
|------|-----|
| Notifications | No user NotificationCenter; no backend feed |
| AI preferences | No dedicated settings API — must show unavailable |
| Security settings | Password/session management not in product API |
| Bulk CMS actions | No supported bulk publish/archive endpoints — UI foundation only, actions disabled with reason |
| Accessibility | Status badges use color + text (good); public pages need consistent focus/ARIA on new dialogs |
| Responsive | Admin tables already responsive; new product pages must avoid overflow at 375px |

---

## Priority remediation (this sprint)

1. Product audit + readiness docs  
2. `/dashboard` from real enrollments + personalization APIs  
3. `/learning` home + reusable learning cards  
4. Global Ctrl+K search palette + API-backed `/search`  
5. NotificationCenter foundation (empty / unread UI; no fake items)  
6. `/settings` with live profile + unavailable stubs for missing APIs  
7. Admin quick actions + CMS selection/bulk toolbar foundation  
8. Shared Page*State components  
9. Frontend product readiness tests  

---

## Explicitly out of scope

Payment, AI agents, knowledge graph, community, new databases, fake metrics, inventing unsupported API behavior.
