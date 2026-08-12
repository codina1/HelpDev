"use client";

import type { ReactNode } from "react";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { ContentFields } from "@/components/admin/content/editor/content-fields";
import type {
  ContentFormErrors,
  ContentFormValues,
  ContentTypeValue,
} from "@/lib/admin/content/content-types";

type ContentFormProps = {
  values: ContentFormValues;
  errors: ContentFormErrors;
  disabled?: boolean;
  lockedType?: ContentTypeValue;
  formTitle?: string;
  afterFields?: ReactNode;
  onChange: (patch: Partial<ContentFormValues>) => void;
  onRegenerateSlug?: () => void;
};

/** Left column of the editor: the content field set in a surface. */
export function ContentForm({
  formTitle = "ویرایشگر محتوا",
  lockedType,
  afterFields,
  ...props
}: ContentFormProps) {
  return (
    <AdminSurface className="space-y-4">
      <h2 className="adm-text text-[14px] font-bold">{formTitle}</h2>
      <ContentFields {...props} lockedType={lockedType} afterFields={afterFields} />
    </AdminSurface>
  );
}
