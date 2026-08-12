import type { Metadata } from "next";
import { AdminModulePlaceholder } from "@/components/admin/page/admin-module-placeholder";

export const metadata: Metadata = { title: "سلامت سیستم" };

export default function AdminOperationsPage() {
  return (
    <AdminModulePlaceholder
      icon="health"
      title="سلامت و عملیات سیستم"
      description="وضعیت سرویس‌ها، Outbox و اطلاعات نسخه."
    />
  );
}
