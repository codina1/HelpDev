import type { Metadata } from "next";
import { WritePageContent } from "@/components/write/write-page-content";

export const metadata: Metadata = { title: "نویسنده شو" };

export default function WritePage() {
  return <WritePageContent />;
}
