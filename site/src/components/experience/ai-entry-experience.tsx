"use client";

import { CommandSearchBox } from "@/components/experience/command-search-box";

type AIEntryExperienceProps = {
  onOpenPalette?: () => void;
  onSubmit?: (query: string) => void;
  className?: string;
};

/**
 * Primary AI entry surface — Ask HelpDev AI prompt interface.
 */
export function AIEntryExperience({
  onOpenPalette,
  onSubmit,
  className = "",
}: AIEntryExperienceProps) {
  return (
    <section aria-labelledby="ai-entry-title" className={className}>
      <h2 id="ai-entry-title" className="sr-only">
        Ask HelpDev AI
      </h2>
      <CommandSearchBox
        onOpenPalette={onOpenPalette}
        onSubmit={onSubmit}
        title="Ask HelpDev AI"
        placeholder="چطور معماری یک سیستم SaaS را طراحی کنم؟"
      />
    </section>
  );
}

/** Alias matching sprint naming for AI command surface. */
export { CommandSearchBox as AICommandBox } from "@/components/experience/command-search-box";
