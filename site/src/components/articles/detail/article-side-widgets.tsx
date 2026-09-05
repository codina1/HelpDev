import Link from "next/link";
import type { ArticleRelatedCourse, ArticleRelatedTool, ArticleRoadmapCta } from "@/data/article-detail";

export function ArticleRelatedTools({ tools }: { tools: readonly ArticleRelatedTool[] }) {
  return (
    <section className="rounded-xl border border-white/[0.08] bg-[#080D1F]/85 p-4 backdrop-blur-xl">
      <h2 className="mb-3 text-[13px] font-extrabold text-white">ابزارهای مرتبط</h2>
      <ul className="space-y-2">
        {tools.map((tool) => (
          <li key={tool.id}>
            <Link
              href={tool.href}
              className="group flex items-center gap-2.5 rounded-xl border border-transparent p-1.5 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
            >
              <span
                className={[
                  "inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-gradient-to-br text-[11px] font-extrabold text-white",
                  tool.iconTone,
                ].join(" ")}
              >
                {tool.name.slice(0, 2)}
              </span>
              <span className="min-w-0 flex-1">
                <span className="block text-[12.5px] font-bold text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                  {tool.name}
                </span>
                <span className="block text-[11px] text-[#64748B]">{tool.description}</span>
              </span>
              <span className="text-[12px] text-[#64748B]" aria-hidden>
                ↗
              </span>
            </Link>
          </li>
        ))}
      </ul>
    </section>
  );
}

export function ArticleRelatedCourseCard({ course }: { course: ArticleRelatedCourse }) {
  return (
    <section className="overflow-hidden rounded-xl border border-white/[0.08] bg-[#080D1F]/85 backdrop-blur-xl">
      <div className={["relative aspect-[16/9] bg-gradient-to-br", course.coverTone].join(" ")}>
        <span className="absolute inset-0 flex items-center justify-center">
          <span className="inline-flex h-11 w-11 items-center justify-center rounded-full bg-black/45 text-white shadow-[0_0_20px_rgba(139,92,246,0.35)] backdrop-blur-sm">
            ▶
          </span>
        </span>
      </div>
      <div className="p-4">
        <h2 className="text-[13px] font-extrabold text-white">{course.title}</h2>
        <p className="mt-1 text-[11.5px] leading-5 text-[#94A3B8]">{course.description}</p>
        <Link
          href={course.href}
          className="mt-3 inline-flex h-8 w-full items-center justify-center rounded-xl border border-[#8B5CF6]/35 bg-[#8B5CF6]/15 text-[12px] font-bold text-[#E9D5FF] no-underline transition hover:bg-[#8B5CF6]/25"
        >
          مشاهده دوره
        </Link>
      </div>
    </section>
  );
}

export function ArticleRoadmapCtaCard({ cta }: { cta: ArticleRoadmapCta }) {
  return (
    <section className="rounded-xl border border-[#8B5CF6]/25 bg-[linear-gradient(160deg,rgba(139,92,246,0.18),rgba(8,13,31,0.95))] p-4 shadow-[0_0_28px_rgba(124,58,237,0.15)]">
      <p className="text-[11px] font-bold tracking-wide text-[#C4B5FD]">مسیر یادگیری</p>
      <h2 className="mt-1 text-[14px] font-extrabold text-white">{cta.title}</h2>
      <p className="mt-1.5 text-[11.5px] leading-5 text-[#CBD5E1]/90">{cta.description}</p>
      <Link
        href={cta.href}
        className="mt-3 inline-flex h-9 w-full items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] text-[12.5px] font-bold text-white no-underline shadow-[0_0_16px_rgba(139,92,246,0.35)] transition hover:brightness-110"
      >
        {cta.ctaLabel}
      </Link>
    </section>
  );
}
