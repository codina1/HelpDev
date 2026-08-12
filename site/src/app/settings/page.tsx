import type { Metadata } from "next";
import { SettingsPageContent } from "@/components/settings/settings-page-content";

export const metadata: Metadata = {
  title: "تنظیمات",
};

export default function SettingsPage() {
  return <SettingsPageContent />;
}
