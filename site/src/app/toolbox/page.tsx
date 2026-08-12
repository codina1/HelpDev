import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import { ToolboxGrid } from "@/components/toolbox/toolbox-grid";
import { TOOLBOX_ITEMS } from "@/data/toolbox";

export const metadata: Metadata = {
  title: "ابزارها",
};

export default function ToolboxPage() {
  return (
    <>
      <PageHeader
        title="ابزارها و چیت‌شیت"
        description="اسنیپت‌های آماده برای کپی و استفاده روزمره."
      />
      <ToolboxGrid items={TOOLBOX_ITEMS} />
    </>
  );
}
