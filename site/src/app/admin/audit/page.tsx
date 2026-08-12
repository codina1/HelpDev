import type { Metadata } from "next";
import { AdminModulePlaceholder } from "@/components/admin/page/admin-module-placeholder";

export const metadata: Metadata = { title: "Audit" };

export default function AdminAuditPage() {
  return (
    <AdminModulePlaceholder
      icon="audit"
      title="گزارش ممیزی (Audit)"
      description="مشاهده رویدادها و فعالیت‌های حساس سامانه."
    />
  );
}
