import Link from "next/link";
import { Badge } from "@/components/ui/public/badge";

export type ContentCardProps = {
  title: string;
  href: string;
  typeLabel?: string;
  summary?: string | null;
  meta?: string | null;
  views?: number;
  className?: string;
};

/**
 * Generic published-content card (articles, news, etc.).
 */
export function ContentCard({
  title,
  href,
  typeLabel,
  summary,
  meta,
  views,
  className = "",
}: ContentCardProps) {
  return (
    <Link
      href={href}
      className={[
        "focus-ring group flex h-full flex-col rounded-2xl border border-[color:var(--border)] bg-[color:var(--surface)] p-4 transition-all duration-300",
        "hover:-translate-y-0.5 hover:border-[color:color-mix(in_srgb,var(--accent)_35%,transparent)] hover:bg-[color:var(--surface-elevated)]",
        "hover:shadow-[0_16px_40px_rgba(49,46,129,0.18)]",
        className,
      ].join(" ")}
    >
      <div className="mb-3 flex items-center justify-between gap-2">
        {typeLabel ? <Badge variant="accent">{typeLabel}</Badge> : <span />}
        {typeof views === "number" ? (
          <span className="text-[11px] text-[color:var(--muted)]">{views.toLocaleString("fa-IR")} بازدید</span>
        ) : null}
      </div>
      <h3 className="line-clamp-2 text-[15px] font-bold leading-6 text-[color:var(--foreground)] transition-colors group-hover:text-violet-200">
        {title}
      </h3>
      {summary ? (
        <p className="mt-2 line-clamp-3 flex-1 text-[13px] leading-6 text-[color:var(--muted)]">
          {summary}
        </p>
      ) : (
        <span className="flex-1" />
      )}
      {meta ? (
        <p className="mt-3 text-[11px] text-[color:var(--muted)]">{meta}</p>
      ) : null}
    </Link>
  );
}
