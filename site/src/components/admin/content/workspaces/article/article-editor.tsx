"use client";

import { ContentStudio } from "@/components/admin/content/editor/content-studio";

/** Article creation uses the same full Content Studio as article editing. */
export function ArticleEditor() {
  return <ContentStudio createType="Article" />;
}
