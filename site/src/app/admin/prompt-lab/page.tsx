import type { Metadata } from "next";
import { AdminModulePlaceholder } from "@/components/admin/page/admin-module-placeholder";

export const metadata: Metadata = { title: "Prompt Lab" };

export default function AdminPromptLabPage() {
  return (
    <AdminModulePlaceholder
      icon="prompt"
      title="مدیریت Prompt Lab"
      description="پرامپت‌ها، نسخه‌ها و دسته‌بندی‌ها."
    />
  );
}
