# Admin Navigation Map

The complete target CMS navigation. Status legend:

- **✅ Implemented** — routed page exists and works in the shell.
- **🟡 Placeholder** — routed page exists in the shell with a clear
  "در حال توسعه" state.
- **⏳ Future** — appears in the sidebar as a disabled "به‌زودی" entry (no route
  yet), reserved for a later phase.

| Group            | Item             | Route                 | Status         |
| ---------------- | ---------------- | --------------------- | -------------- |
| داشبورد          | نمای کلی         | `/admin`              | ✅ Implemented |
| محتوا            | همه محتواها      | `/admin/content`      | 🟡 Placeholder |
| محتوا            | ایجاد محتوا      | `/admin/content/new`  | 🟡 Placeholder |
| محتوا            | تقویم انتشار     | —                     | ⏳ Future      |
| محتوا            | دسته‌بندی‌ها     | —                     | ⏳ Future      |
| محتوا            | برچسب‌ها         | —                     | ⏳ Future      |
| محتوا            | رسانه‌ها         | —                     | ⏳ Future      |
| محتوا            | مرکز SEO         | —                     | ⏳ Future      |
| آموزش            | دوره‌ها          | `/admin/learning`     | 🟡 Placeholder |
| آموزش            | فصل‌ها و درس‌ها  | —                     | ⏳ Future      |
| آموزش            | ثبت‌نام‌ها       | —                     | ⏳ Future      |
| آموزش            | پیشرفت کاربران   | —                     | ⏳ Future      |
| ابزارها          | همه ابزارها      | `/admin/toolbox`      | 🟡 Placeholder |
| ابزارها          | دسته‌بندی‌ها     | —                     | ⏳ Future      |
| ابزارها          | اجرای ابزارها    | —                     | ⏳ Future      |
| Prompt Lab       | همه پرامپت‌ها    | `/admin/prompt-lab`   | ✅ Implemented |
| Prompt Lab       | بازبینی پرامپت‌ها | `/admin/prompts`     | ✅ Implemented |
| Prompt Lab       | نسخه‌ها          | —                     | ⏳ Future      |
| Prompt Lab       | دسته‌بندی‌ها     | —                     | ⏳ Future      |
| کاربران و دسترسی | کاربران          | `/admin/users`        | ✅ Implemented |
| کاربران و دسترسی | نقش‌ها           | —                     | ⏳ Future      |
| کاربران و دسترسی | دسترسی‌ها        | —                     | ⏳ Future      |
| کاربران و دسترسی | فعالیت کاربران   | —                     | ⏳ Future      |
| تحلیل‌ها         | نمای کلی         | `/admin/analytics`    | 🟡 Placeholder |
| تحلیل‌ها         | محتوا            | —                     | ⏳ Future      |
| تحلیل‌ها         | جستجو            | —                     | ⏳ Future      |
| تحلیل‌ها         | آموزش            | —                     | ⏳ Future      |
| تحلیل‌ها         | ابزارها          | —                     | ⏳ Future      |
| تحلیل‌ها         | Prompt Lab       | —                     | ⏳ Future      |
| سیستم            | اعلان‌ها         | —                     | ⏳ Future      |
| سیستم            | Feature Flags    | —                     | ⏳ Future      |
| سیستم            | تنظیمات          | `/admin/settings`     | 🟡 Placeholder |
| سیستم            | Audit            | `/admin/audit`        | 🟡 Placeholder |
| سیستم            | سلامت سیستم      | `/admin/operations`   | 🟡 Placeholder |
| سیستم            | Outbox           | —                     | ⏳ Future      |
| سیستم            | نسخه و انتشار    | —                     | ⏳ Future      |

## Notes

- **Implemented** pages call existing canonical `/api/v1` endpoints via the typed
  API client (dashboard: `admin/dashboard`; users: `admin/users`,
  `admin/users/{id}`). No new backend endpoints were added.
- **Placeholder** pages render `AdminModulePlaceholder` inside the shell — a real
  header + "در حال توسعه" empty state, not a fake functional screen.
- **Future** entries are disabled sidebar rows and never produce dead links or
  404s.
- The single source of truth for this map is `src/lib/admin/navigation.ts`.
