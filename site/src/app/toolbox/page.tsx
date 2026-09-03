import type { Metadata } from "next";
import { ToolHero } from "@/components/tools/tool-hero";
import { ToolsCatalog } from "@/components/tools/tools-catalog";

export const metadata: Metadata = {
  title: "ابزارها",
  description:
    "مجموعه‌ای منتخب از بهترین ابزارها و سرویس‌هایی که به توسعه‌دهندگان کمک می‌کنند سریع‌تر، هوشمندتر و با کیفیت‌تر کار کنند.",
};

/**
 * Public Tools marketplace — Hero · Categories · Sidebar · Tool Grid.
 */
export default function ToolboxPage() {
  return (
    <div className="bg-[#070b18] pb-8 text-[#E5E7EB]">
      <ToolHero />
      <ToolsCatalog />
    </div>
  );
}
