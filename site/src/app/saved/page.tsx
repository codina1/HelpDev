import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/ui/placeholder-page";

export const metadata: Metadata = { title: "ذخیره‌شده‌ها" };

export default function SavedPage() {
  return (
    <PlaceholderPage
      title="ذخیره‌شده‌ها"
      description="محتوای ذخیره‌شده شما در این بخش قرار می‌گیرد."
    />
  );
}
