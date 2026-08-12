import Link from "next/link";
import { Badge } from "@/components/ui/public/badge";
import type { TocHeading } from "@/lib/public/content-helpers";

type ArticleTocProps = {
  headings: TocHeading[];
};

export function ArticleToc({ headings }: ArticleTocProps) {
  if (headings.length === 0) {
    return (
      <aside
        className="rounded-2xl border border-dashed border-[color:var(--border-strong)] p-4 text-[12px] text-[color:var(--muted)]"
        aria-label="فهرست مطالب"
      >
        فهرست مطالب پس از افزودن عناوین Markdown (## / ###) نمایش داده می‌شود.
      </aside>
    );
  }

  return (
    <nav
      className="rounded-2xl border border-[color:var(--border)] bg-[color:var(--surface)] p-4"
      aria-label="فهرست مطالب"
    >
      <p className="mb-3 text-[12px] font-bold text-[color:var(--muted)]">فهرست مطالب</p>
      <ol className="space-y-1.5">
        {headings.map((heading) => (
          <li key={heading.id} className={heading.level === 3 ? "ps-3" : ""}>
            <a
              href={`#${heading.id}`}
              className="focus-ring block rounded px-1 py-0.5 text-[13px] text-[color:var(--foreground)]/85 hover:text-violet-300"
            >
              {heading.text}
            </a>
          </li>
        ))}
      </ol>
    </nav>
  );
}

type RelatedPlaceholderProps = {
  currentSlug: string;
};

export function RelatedContentPlaceholder({ currentSlug }: RelatedPlaceholderProps) {
  return (
    <section
      className="rounded-2xl border border-[color:var(--border)] bg-[color:var(--surface)] p-4"
      aria-labelledby="related-title"
    >
      <div className="mb-2 flex items-center gap-2">
        <h2 id="related-title" className="text-[13px] font-bold text-[color:var(--foreground)]">
          محتوای مرتبط
        </h2>
        <Badge variant="muted">به‌زودی</Badge>
      </div>
      <p className="text-[12px] leading-6 text-[color:var(--muted)]">
        پیشنهاد مرتبط از Knowledge / Search API در اسپرینت‌های بعدی به این بخش وصل می‌شود.
        فعلاً می‌توانید در{" "}
        <Link href={`/search?q=${encodeURIComponent(currentSlug)}`} className="text-violet-300 hover:underline">
          جستجو
        </Link>{" "}
        موارد مشابه را پیدا کنید.
      </p>
    </section>
  );
}
