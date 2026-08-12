import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/ui/placeholder-page";

export const metadata: Metadata = { title: "Dev Starter Kit" };

export default function StarterKitPage() {
  return (
    <PlaceholderPage
      title="Dev Starter Kit"
      description="قالب‌های آماده برای شروع سریع پروژه‌های واقعی."
    />
  );
}
