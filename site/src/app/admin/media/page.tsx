import type { Metadata } from "next";
import { MediaWorkspace } from "@/components/admin/media/media-workspace";

export const metadata: Metadata = { title: "رسانه‌ها" };

export default function AdminMediaPage() {
  return <MediaWorkspace />;
}
