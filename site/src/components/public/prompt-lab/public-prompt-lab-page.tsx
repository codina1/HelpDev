"use client";

import { PromptLabCatalog } from "@/components/prompt-lab/prompt-lab-catalog";
import { PromptLabHero } from "@/components/prompt-lab/prompt-lab-hero";

/**
 * Public Prompt Lab homepage — GET /api/v1/prompts.
 * Layout matches the HelpDev Prompt Lab reference: Hero · Categories · Sidebar · Grid.
 */
export function PublicPromptLabPage() {
  return (
    <div className="bg-[#070b18] pb-20 text-[#E5E7EB]">
      <PromptLabHero />
      <PromptLabCatalog />
    </div>
  );
}
