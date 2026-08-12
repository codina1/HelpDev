import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import { PromptLab } from "@/components/prompt-lab/prompt-lab";

export const metadata: Metadata = {
  title: "Prompt Lab",
};

export default function PromptLabPage() {
  return (
    <>
      <PageHeader
        title="Prompt Lab"
        description="درخواست کوتاه خود را به پرامپت حرفه‌ای و ساخت‌یافته تبدیل کنید."
      />
      <PromptLab />
    </>
  );
}
