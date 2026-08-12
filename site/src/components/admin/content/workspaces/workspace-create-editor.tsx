"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useCallback, useState, type ReactNode } from "react";
import {
  hasFormErrors,
  slugify,
  validateContentForm,
} from "@/lib/admin/content/content-mappers";
import { useCreateContent } from "@/lib/admin/content/content-hooks";
import type {
  ContentFormErrors,
  ContentFormValues,
  ContentStatusValue,
  ContentTypeValue,
} from "@/lib/admin/content/content-types";
import type { ContentWorkspaceDefinition } from "@/lib/admin/content/factory";
import { adminContentItemRoute } from "@/lib/admin/routes";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ContentForm } from "@/components/admin/content/editor/content-form";
import { ContentPreviewPanel } from "@/components/admin/content/editor/content-preview-panel";
import { PublishPanel } from "@/components/admin/content/editor/publish-panel";
import { WorkspaceHeader } from "@/components/admin/content/workspaces/workspace-header";

type WorkspaceCreateEditorProps = {
  workspace: ContentWorkspaceDefinition;
  contentType: ContentTypeValue;
  formTitle?: string;
  afterFields?: ReactNode;
  /** Where to send the user after create (default: legacy content detail). */
  successPath?: (id: string) => string;
  /** Optional post-create hook (e.g. save type-specific metadata) before redirect. */
  afterCreate?: (id: string) => Promise<void>;
};

/**
 * Shared create flow for typed workspaces.
 * Always POSTs via existing createContent API with a locked ContentType.
 */
export function WorkspaceCreateEditor({
  workspace,
  contentType,
  formTitle,
  afterFields,
  successPath = adminContentItemRoute,
  afterCreate,
}: WorkspaceCreateEditorProps) {
  const router = useRouter();
  const create = useCreateContent();
  const [values, setValues] = useState<ContentFormValues>(() => ({
    title: "",
    slug: "",
    type: contentType,
    body: "",
    status: "Draft",
    excerpt: "",
    coverImage: "",
  }));
  const [errors, setErrors] = useState<ContentFormErrors>({});
  const [slugTouched, setSlugTouched] = useState(false);

  const onChange = useCallback(
    (patch: Partial<ContentFormValues>) => {
      setValues((prev) => {
        const next = { ...prev, ...patch, type: contentType };
        if (patch.slug !== undefined) setSlugTouched(true);
        if (patch.title !== undefined && !slugTouched) {
          next.slug = slugify(patch.title);
        }
        return next;
      });
    },
    [contentType, slugTouched],
  );

  const onRegenerateSlug = useCallback(() => {
    setSlugTouched(true);
    setValues((prev) => ({ ...prev, slug: slugify(prev.title), type: contentType }));
  }, [contentType]);

  const submit = useCallback(
    async (status: ContentStatusValue) => {
      const next = { ...values, type: contentType, status };
      const validation = validateContentForm(next);
      setErrors(validation);
      if (hasFormErrors(validation)) return;

      try {
        const created = await create.create({
          title: next.title.trim(),
          slug: next.slug.trim(),
          body: next.body,
          type: contentType,
          status,
        });
        if (afterCreate) {
          await afterCreate(created.id);
        }
        router.push(successPath(created.id));
      } catch {
        // Surfaced via create.error
      }
    },
    [values, contentType, create, router, successPath, afterCreate],
  );

  return (
    <div className="space-y-6">
      <WorkspaceHeader
        workspace={{ ...workspace, title: workspace.createTitle }}
        showCreate={false}
        secondaryActions={
          <Link
            href={workspace.listHref}
            className="adm-btn adm-btn-outline adm-focus inline-flex items-center gap-1.5"
          >
            <AdminIcon name="chevron" size={16} />
            بازگشت به فهرست
          </Link>
        }
      />

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <ContentForm
          values={values}
          errors={errors}
          disabled={create.submitting}
          lockedType={contentType}
          formTitle={formTitle ?? workspace.createTitle}
          afterFields={afterFields}
          onChange={onChange}
          onRegenerateSlug={onRegenerateSlug}
        />
        <div className="space-y-4">
          <ContentPreviewPanel values={values} />
          <PublishPanel
            status={values.status}
            submitting={create.submitting}
            canMutate
            error={create.error}
            onSaveDraft={() => void submit("Draft")}
            onPublish={() => void submit("Published")}
          />
        </div>
      </div>
    </div>
  );
}
