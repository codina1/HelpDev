"use client";

import { useEffect, useState } from "react";
import type { TocHeading } from "@/lib/public/content-helpers";

type ArticleTocNavProps = {
  headings: TocHeading[];
};

/** Sticky TOC with purple rail + active section highlight. */
export function ArticleTocNav({ headings }: ArticleTocNavProps) {
  const [activeId, setActiveId] = useState<string>(headings[0]?.id ?? "");

  useEffect(() => {
    if (headings.length === 0) return;

    const nodes = headings
      .map((heading) => document.getElementById(heading.id))
      .filter((node): node is HTMLElement => Boolean(node));

    if (nodes.length === 0) return;

    const observer = new IntersectionObserver(
      (entries) => {
        const visible = entries
          .filter((entry) => entry.isIntersecting)
          .sort((a, b) => b.intersectionRatio - a.intersectionRatio);
        if (visible[0]?.target?.id) {
          setActiveId(visible[0].target.id);
        }
      },
      {
        rootMargin: "-20% 0px -65% 0px",
        threshold: [0, 0.25, 0.5, 1],
      },
    );

    nodes.forEach((node) => observer.observe(node));
    return () => observer.disconnect();
  }, [headings]);

  if (headings.length === 0) {
    return (
      <aside className="rounded-xl border border-dashed border-white/[0.1] bg-[#080D1F]/80 p-4 text-[12px] text-[#94A3B8]">
        فهرست مطالب پس از افزودن عناوین (## / ###) نمایش داده می‌شود.
      </aside>
    );
  }

  return (
    <nav
      className="rounded-xl border border-white/[0.08] bg-[#080D1F]/85 p-4 shadow-[0_0_28px_rgba(124,58,237,0.08)] backdrop-blur-xl"
      aria-label="در این مقاله"
    >
      <p className="mb-3 text-[13px] font-extrabold text-white">در این مقاله</p>
      <ol className="relative space-y-0.5 border-e-2 border-[#8B5CF6]/35 pe-3">
        {headings.map((heading) => {
          const active = heading.id === activeId;
          return (
            <li key={heading.id} className={heading.level === 3 ? "pe-2" : ""}>
              <a
                href={`#${heading.id}`}
                className={[
                  "focus-ring relative block rounded-lg px-2.5 py-1.5 text-[12.5px] leading-6 transition",
                  active
                    ? "bg-[#8B5CF6]/15 font-bold text-[#E9D5FF] shadow-[0_0_16px_rgba(139,92,246,0.25)]"
                    : "text-[#94A3B8] hover:bg-white/[0.04] hover:text-white",
                ].join(" ")}
              >
                {active ? (
                  <span
                    className="absolute -end-[15px] top-1/2 h-5 w-[3px] -translate-y-1/2 rounded-full bg-[#8B5CF6] shadow-[0_0_10px_rgba(139,92,246,0.8)]"
                    aria-hidden
                  />
                ) : null}
                {heading.text}
              </a>
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
