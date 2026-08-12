import Link from "next/link";
import { Badge } from "@/components/ui/public/badge";

export type RoadmapCardProps = {
  title: string;
  href: string;
  summary?: string | null;
  level?: string | null;
  stepCount?: number | null;
  className?: string;
};

/**
 * Learning / content roadmap card for public showcase.
 */
export function RoadmapCard({
  title,
  href,
  summary,
  level,
  stepCount,
  className = "",
}: RoadmapCardProps) {
  return (
    <Link
      href={href}
      className={[
        "focus-ring group flex h-full flex-col rounded-2xl border border-[color:var(--border)] bg-[color:var(--surface)] p-4 transition-all duration-300",
        "hover:-translate-y-0.5 hover:border-emerald-500/35 hover:shadow-[0_16px_40px_rgba(16,185,129,0.12)]",
        className,
      ].join(" ")}
    >
      <div className="mb-3 flex flex-wrap items-center gap-2">
        <Badge variant="success">نقشه راه</Badge>
        {level ? <Badge variant="outline">{level}</Badge> : null}
        {typeof stepCount === "number" ? (
          <span className="text-[11px] text-[color:var(--muted)]">
            {stepCount.toLocaleString("fa-IR")} گام
          </span>
        ) : null}
      </div>
      <h3 className="text-[15px] font-bold leading-6 text-[color:var(--foreground)] group-hover:text-emerald-200">
        {title}
      </h3>
      {summary ? (
        <p className="mt-2 line-clamp-3 flex-1 text-[13px] leading-6 text-[color:var(--muted)]">
          {summary}
        </p>
      ) : (
        <span className="flex-1" />
      )}
      <span className="mt-4 text-[12px] font-semibold text-emerald-300/90">شروع مسیر ←</span>
    </Link>
  );
}
