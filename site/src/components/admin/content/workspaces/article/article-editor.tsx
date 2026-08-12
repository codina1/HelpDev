"use client";

import { getWorkspaceByKey } from "@/lib/admin/content/factory";
import { adminContentArticleRoute } from "@/lib/admin/routes";
import { WorkspaceCreateEditor } from "@/components/admin/content/workspaces/workspace-create-editor";
import { WorkspaceNote } from "@/components/admin/content/workspaces/future-capability-list";

const workspace = getWorkspaceByKey("article");

/** Article workspace create — ContentType fixed to Article; SEO/media after save in Studio. */
export function ArticleEditor() {
  return (
    <WorkspaceCreateEditor
      workspace={workspace}
      contentType="Article"
      formTitle="ویرایشگر مقاله"
      successPath={(id) => `${adminContentArticleRoute(id)}`}
      afterFields={
        <WorkspaceNote>
          پس از ذخیره می‌توانید در استودیوی محتوا، SEO و تصویر کاور را تکمیل کنید. انتخاب نوع
          محتوا در این فضای کار غیرفعال است.
        </WorkspaceNote>
      }
    />
  );
}
