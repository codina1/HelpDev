import type { Metadata } from "next";
import { AdminModulePlaceholder } from "@/components/admin/page/admin-module-placeholder";

export const metadata: Metadata = { title: "تحلیل‌ها" };

export default function AdminAnalyticsPage() {
  return (
    <AdminModulePlaceholder
      icon="analytics"
      title="تحلیل‌ها"
      description="آمار محتوا، جستجو، آموزش، ابزارها و Prompt Lab."
    />
  );
}
