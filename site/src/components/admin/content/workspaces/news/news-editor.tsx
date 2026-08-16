"use client";

import { ContentStudio } from "@/components/admin/content/editor/content-studio";

/** News creation uses the same full Studio and saves news metadata on first create. */
export function NewsEditor() {
  return <ContentStudio createType="News" />;
}
