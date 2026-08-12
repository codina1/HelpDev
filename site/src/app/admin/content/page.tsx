import type { Metadata } from "next";
import { ContentPlatformHub } from "@/components/admin/content/content-platform-hub";

export const metadata: Metadata = { title: "پلتفرم محتوا" };

export default function AdminContentPage() {
  return <ContentPlatformHub />;
}
