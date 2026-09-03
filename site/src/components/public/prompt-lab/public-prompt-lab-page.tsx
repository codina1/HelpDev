"use client";

import { PromptLabCatalog } from "@/components/prompt-lab/prompt-lab-catalog";
import { PromptLabHero } from "@/components/prompt-lab/prompt-lab-hero";

/**
 * Public Prompt Lab homepage — Hero · Categories · Sidebar · Prompt Grid.
 */
export function PublicPromptLabPage() {
  return (
    <div className="bg-[#070b18] pb-[60px] text-[#E5E7EB]">
      <PromptLabHero />
      <PromptLabCatalog />
    </div>
  );
}
