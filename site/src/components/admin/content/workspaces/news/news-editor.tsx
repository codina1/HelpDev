"use client";

import { useCallback, useState } from "react";
import { useAuth } from "@/components/auth";
import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { ADMIN_ROUTES } from "@/lib/admin/routes";
import { updateNewsMetadata } from "@/lib/admin/content/content-api";
import {
  buildNewsPayload,
  hasFormErrors,
  validateNewsForm,
} from "@/lib/admin/content/content-mappers";
import {
  EMPTY_NEWS_FORM,
  type NewsFormErrors,
  type NewsFormValues,
} from "@/lib/admin/content/content-types";
import { WorkspaceCreateEditor } from "@/components/admin/content/workspaces/workspace-create-editor";
import { WorkspaceNote } from "@/components/admin/content/workspaces/future-capability-list";
import { NewsSettingsFields } from "@/components/admin/content/workspaces/news/news-settings-fields";
import { AdminSurface } from "@/components/admin/page/admin-surface";

const workspace = getWorkspaceByKey("news");

export function NewsEditor() {
  const { token } = useAuth();
  const [newsValues, setNewsValues] = useState<NewsFormValues>(() => ({
    ...EMPTY_NEWS_FORM,
    newsDateUtc: new Date().toISOString().slice(0, 16),
  }));
  const [newsErrors, setNewsErrors] = useState<NewsFormErrors>({});

  const afterCreate = useCallback(
    async (id: string) => {
      const validation = validateNewsForm(newsValues);
      setNewsErrors(validation);
      if (hasFormErrors(validation)) {
        throw new Error("اعتبارسنجی تنظیمات خبر ناموفق بود.");
      }
      if (!token) {
        throw new Error("برای ذخیره تنظیمات خبر باید وارد شوید.");
      }
      await updateNewsMetadata(token, id, buildNewsPayload(newsValues));
    },
    [newsValues, token],
  );

  return (
    <WorkspaceCreateEditor
      workspace={workspace}
      contentType="News"
      formTitle="ویرایشگر خبر"
      successPath={(id) => `${ADMIN_ROUTES.contentNews}/${encodeURIComponent(id)}`}
      afterCreate={afterCreate}
      afterFields={
        <div className="space-y-3">
          <AdminSurface className="p-4">
            <NewsSettingsFields
              values={newsValues}
              errors={newsErrors}
              onChange={(patch) => setNewsValues((prev) => ({ ...prev, ...patch }))}
              hideSave
              title="اطلاعات خبر"
            />
          </AdminSurface>
          <WorkspaceNote>
            پس از ایجاد محتوا، منبع و اولویت از طریق API متادیتای خبر ذخیره می‌شوند. سئو در
            استودیو تکمیل می‌شود.
          </WorkspaceNote>
        </div>
      }
    />
  );
}
