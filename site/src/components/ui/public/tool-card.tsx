import Link from "next/link";
import { Badge } from "@/components/ui/public/badge";

export type ToolCardProps = {
  title: string;
  href: string;
  summary?: string | null;
  category?: string | null;
  status?: string | null;
  className?: string;
};

/**
 * Tool / toolbox showcase card for public surfaces.
 */
export function ToolCard({
  title,
  href,
  summary,
  category,
  status,
  className = "",
}: ToolCardProps) {
  return (
    <Link
      href={href}
      className={[
        "focus-ring group relative flex h-full flex-col overflow-hidden rounded-2xl border border-[color:var(--border)] bg-[color:var(--surface)] p-4 transition-all duration-300",
        "hover:-translate-y-0.5 hover:border-sky-500/35 hover:shadow-[0_16px_40px_rgba(14,165,233,0.12)]",
        className,
      ].join(" ")}
    >
      <div
        className="pointer-events-none absolute inset-x-0 top-0 h-px bg-gradient-to-l from-transparent via-sky-400/60 to-transparent opacity-0 transition-opacity group-hover:opacity-100"
        aria-hidden
      />
      <div className="mb-3 flex items-center gap-2">
        <span
          className="flex h-9 w-9 items-center justify-center rounded-xl bg-sky-500/15 text-sky-300"
          aria-hidden
        >
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
            <path d="M14.7 6.3a1 1 0 0 0 0 1.4l1.6 1.6a1 1 0 0 0 1.4 0l3.77-3.77a6 6 0 0 1-7.94 7.94l-6.91 6.91a2.12 2.12 0 0 1-3-3l6.91-6.91a6 6 0 0 1 7.94-7.94l-3.76 3.76z" />
          </svg>
        </span>
        {category ? <Badge variant="outline">{category}</Badge> : null}
        {status && status.toLowerCase() !== "published" ? (
          <Badge variant="muted">{status}</Badge>
        ) : null}
      </div>
      <h3 className="text-[15px] font-bold text-[color:var(--foreground)] group-hover:text-sky-200">
        {title}
      </h3>
      {summary ? (
        <p className="mt-2 line-clamp-2 text-[13px] leading-6 text-[color:var(--muted)]">{summary}</p>
      ) : null}
      <span className="mt-auto pt-4 text-[12px] font-semibold text-sky-300/90 opacity-0 transition-opacity group-hover:opacity-100">
        مشاهده ابزار ←
      </span>
    </Link>
  );
}
