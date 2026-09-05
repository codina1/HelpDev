"use client";

import { useEffect, useState } from "react";

/** Thin purple reading progress bar fixed under the site header. */
export function ArticleReadingProgress() {
  const [progress, setProgress] = useState(0);

  useEffect(() => {
    function onScroll() {
      const el = document.documentElement;
      const max = el.scrollHeight - el.clientHeight;
      setProgress(max > 0 ? Math.min(100, Math.max(0, (el.scrollTop / max) * 100)) : 0);
    }
    onScroll();
    window.addEventListener("scroll", onScroll, { passive: true });
    return () => window.removeEventListener("scroll", onScroll);
  }, []);

  return (
    <div
      className="pointer-events-none fixed inset-x-0 top-0 z-[60] h-[3px] bg-transparent"
      aria-hidden
    >
      <div
        className="h-full bg-gradient-to-l from-[#8B5CF6] via-[#A78BFA] to-[#2563EB] shadow-[0_0_12px_rgba(139,92,246,0.7)] transition-[width] duration-150 ease-out"
        style={{ width: `${progress}%` }}
      />
    </div>
  );
}
