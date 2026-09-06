"use client";

import type { ArticleDetailAuthor } from "@/data/article-detail";

type ArticleAuthorProps = {
  author: ArticleDetailAuthor;
};

export function ArticleAuthor({ author }: ArticleAuthorProps) {
  return (
    <section className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/9 px-4 py-4 shadow-[0_0_28px_rgba(139,92,246,0.1)] sm:px-5">
      <div className="flex flex-col gap-3 sm:flex-row sm:items-center">
        <span className="inline-flex h-14 w-14 shrink-0 items-center justify-center overflow-hidden rounded-full border border-white/[0.1] bg-gradient-to-br from-[#8B5CF6]/45 to-[#2563EB]/25 text-[14px] font-bold text-white">
          {author.avatarUrl ? (
            <img src={author.avatarUrl} alt="" className="h-full w-full object-cover" />
          ) : (
            author.initials
          )}
        </span>
        <div className="min-w-0 flex-1">
          <div className="flex flex-wrap items-center gap-x-3 gap-y-1">
            <h3 className="text-[15px] font-extrabold text-white">{author.name}</h3>
            <p className="text-[12px] font-semibold text-[#C4B5FD]">{author.role}</p>
          </div>
          <p className="mt-1 line-clamp-2 text-[12.5px] leading-6 text-[#94A3B8]">{author.bio}</p>
        </div>
        <div className="flex shrink-0 gap-2">
          {[
            { label: "GitHub", href: "https://github.com" },
            { label: "LinkedIn", href: "https://linkedin.com" },
            { label: "X", href: "https://twitter.com" },
          ].map((item) => (
            <a
              key={item.label}
              href={item.href}
              target="_blank"
              rel="noopener noreferrer"
              aria-label={item.label}
              className="focus-ring inline-flex h-8 w-8 items-center justify-center rounded-full border border-white/[0.08] bg-[#070B18] text-[10px] font-bold text-[#94A3B8] transition hover:border-[#8B5CF6]/4 hover:text-white"
            >
              {item.label === "GitHub" ? "GH" : item.label === "LinkedIn" ? "in" : "X"}
            </a>
          ))}
        </div>
      </div>
    </section>
  );
}
