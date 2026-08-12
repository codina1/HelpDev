import type { Metadata } from "next";
import { AdminModulePlaceholder } from "@/components/admin/page/admin-module-placeholder";

export const metadata: Metadata = { title: "آموزش" };

export default function AdminLearningPage() {
  return (
    <AdminModulePlaceholder
      icon="learning"
      title="مدیریت آموزش"
      description="دوره‌ها، فصل‌ها، درس‌ها و ثبت‌نام‌ها."
    />
  );
}
