"use client";

import { useRouter } from "next/navigation";
import { useState } from "react";
import { CommandSearchBox } from "@/components/experience/command-search-box";
import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { PublicSection } from "@/components/ui/public/v2";
import { GlobalSearchPalette } from "@/components/search/global-search-palette";

/**
 * Ask HelpDev AI section — prompt interface + Ctrl+K palette (replaces plain keyword search chrome).
 */
export function KnowledgeSearchSection() {
  const router = useRouter();
  const [paletteOpen, setPaletteOpen] = useState(false);

  return (
    <PublicSection className="ds-slide" aria-labelledby="knowledge-search-title">
      <PremiumSectionHeader
        eyebrow="Ask HelpDev AI"
        title="Ask HelpDev AI"
        description="پرسش مهندسی را بنویسید — مسیر، ابزار و دانش مرتبط از پایگاه HelpDev پیشنهاد می‌شود"
        titleId="knowledge-search-title"
        icon={<span aria-hidden>✦</span>}
      />
      <CommandSearchBox
        onOpenPalette={() => setPaletteOpen(true)}
        onSubmit={(q) => router.push(`/search?q=${encodeURIComponent(q)}`)}
        title="Ask HelpDev AI"
        placeholder="چطور معماری یک سیستم SaaS را طراحی کنم؟"
      />
      <GlobalSearchPalette open={paletteOpen} onOpenChange={setPaletteOpen} />
    </PublicSection>
  );
}
