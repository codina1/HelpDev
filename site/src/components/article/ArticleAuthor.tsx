"use client";

import type { ArticleDetailAuthor } from "@/data/article-detail";

type ArticleAuthorProps = {
  author: ArticleDetailAuthor;
};

export function ArticleAuthor({ author }: ArticleAuthorProps) {
  return (
    <section className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/9 p-5 shadow-[0_0_28px_rgba(139,92,246,0.1)]">
      <p className="text-[12px] font-bold text-[#94A3B8]">درباره نویسنده</p>
      <div className="mt-3 flex items-start gap-3">
        <span className="inline-flex h-14 w-14 shrink-0 items-center justify-center overflow-hidden rounded-full border border-white/[0.1] bg-gradient-to-br from-[#8B5CF6]/45 to-[#2563EB]/25 text-[14px] font-bold text-white">
          {author.avatarUrl ? (
            <img src={author.avatarUrl} alt="" className="h-full w-full object-cover" />
          ) : (
            author.initials
          )}
        </span>
        <div className="min-w-0 flex-1">
          <h3 className="text-[16px] font-extrabold text-white">{author.name}</h3>
          <p className="mt-0.5 text-[12.5px] font-semibold text-[#C4B5FD]">{author.role}</p>
          <p className="mt-2 text-[13px] leading-7 text-[#94A3B8]">{author.bio}</p>
          <div className="mt-3 flex gap-2">
            {[
              { label: "GitHub", href: "https://github.com" },
              { label: "LinkedIn", href: "https://linkedin.com" },
              { label: "Twitter", href: "https://twitter.com" },
            ].map((item) => (
              <a
                key={item.label}
                href={item.href}
                target="_blank"
                rel="noopener noreferrer"
                className="inline-flex h-8 items-center rounded-lg border border-white/[0.08] bg-[#070B18] px-2.5 text-[11px] font-bold text-[#94A3B8] transition hover:border-[#8B5CF6]/4 hover:text-white"
              >
                {item.label}
              </a>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
