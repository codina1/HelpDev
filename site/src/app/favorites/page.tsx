import type { Metadata } from "next";
import { PlaceholderPage } from "@/components/ui/placeholder-page";

export const metadata: Metadata = { title: "علاقه‌مندی‌ها" };

export default function FavoritesPage() {
  return (
    <PlaceholderPage
      title="علاقه‌مندی‌ها"
      description="مواردی که علامت‌گذاری کرده‌اید اینجا نمایش داده می‌شود."
    />
  );
}
