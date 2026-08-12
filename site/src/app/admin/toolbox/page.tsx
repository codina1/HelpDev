import type { Metadata } from "next";
import { AdminModulePlaceholder } from "@/components/admin/page/admin-module-placeholder";

export const metadata: Metadata = { title: "ابزارها" };

export default function AdminToolboxPage() {
  return (
    <AdminModulePlaceholder
      icon="toolbox"
      title="مدیریت ابزارها"
      description="کاتالوگ ابزارها، دسته‌بندی‌ها و اجراها."
    />
  );
}
