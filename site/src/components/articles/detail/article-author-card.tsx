import Link from "next/link";
import type { ArticleDetailAuthor } from "@/data/article-detail";

type ArticleAuthorCardProps = {
  author: ArticleDetailAuthor;
};

function SocialIcon({ name }: { name: "linkedin" | "github" | "x" }) {
  if (name === "github") {
    return (
      <svg viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="currentColor" aria-hidden>
        <path d="M12 2a10 10 0 0 0-3.16 19.49c.5.09.68-.22.68-.48v-1.7c-2.78.6-3.37-1.34-3.37-1.34-.45-1.16-1.1-1.47-1.1-1.47-.9-.62.07-.61.07-.61 1 .07 1.53 1.03 1.53 1.03.89 1.52 2.34 1.08 2.91.83.09-.65.35-1.08.63-1.33-2.22-.25-4.55-1.11-4.55-4.94 0-1.09.39-1.98 1.03-2.68-.1-.25-.45-1.27.1-2.64 0 0 .84-.27 2.75 1.02A9.56 9.56 0 0 1 12 6.8c.85 0 1.7.11 2.5.34 1.9-1.29 2.74-1.02 2.74-1.02.55 1.37.2 2.39.1 2.64.64.7 1.03 1.59 1.03 2.68 0 3.84-2.34 4.69-4.57 4.93.36.31.68.92.68 1.86v2.76c0 .27.18.58.69.48A10 10 0 0 0 12 2Z" />
      </svg>
    );
  }
  if (name === "linkedin") {
    return (
      <svg viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="currentColor" aria-hidden>
        <path d="M6.94 8.5H3.56V21h3.38V8.5ZM5.25 3a1.94 1.94 0 1 0 0 3.88 1.94 1.94 0 0 0 0-3.88ZM20.44 21h-3.37v-6.07c0-1.45-.03-3.31-2.02-3.31-2.02 0-2.33 1.58-2.33 3.2V21H9.35V8.5h3.24v1.71h.05c.45-.85 1.55-1.75 3.19-1.75 3.41 0 4.61 2.24 4.61 5.15V21Z" />
      </svg>
    );
  }
  return (
    <svg viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="currentColor" aria-hidden>
      <path d="M18.244 3H21l-6.53 7.46L22 21h-6.19l-4.85-6.34L5.4 21H2.64l7-8  -7-10h6.34l4.38 5.82L18.244 3Zm-1.09 16.2h1.72L7.01 4.7H5.16l11.994 14.5Z" />
    </svg>
  );
}

export function ArticleAuthorCard({ author }: ArticleAuthorCardProps) {
  return (
    <aside className="rounded-xl border border-white/[0.08] bg-[#080D1F]/85 p-4 text-center shadow-[0_0_28px_rgba(124,58,237,0.1)] backdrop-blur-xl">
      <p className="mb-3 text-start text-[13px] font-extrabold text-white">درباره نویسنده</p>
      <div className="mx-auto flex h-16 w-16 items-center justify-center overflow-hidden rounded-full border border-[#8B5CF6]/40 bg-gradient-to-br from-[#8B5CF6]/40 to-[#2563EB]/30 text-[18px] font-extrabold text-white shadow-[0_0_24px_rgba(139,92,246,0.35)]">
        {author.avatarUrl ? (
          <img src={author.avatarUrl} alt="" className="h-full w-full object-cover" />
        ) : (
          author.initials
        )}
      </div>
      <h2 className="mt-3 text-[15px] font-extrabold text-white">{author.name}</h2>
      <p className="mt-0.5 text-[12px] font-semibold text-[#A78BFA]">{author.role}</p>
      <p className="mt-2 text-[12px] leading-6 text-[#94A3B8]">{author.bio}</p>

      <div className="mt-3 flex items-center justify-center gap-2">
        {(["linkedin", "github", "x"] as const).map((name) => (
          <a
            key={name}
            href="#"
            className="focus-ring inline-flex h-8 w-8 items-center justify-center rounded-lg border border-white/[0.08] bg-[#0B1224] text-[#94A3B8] transition hover:border-[#8B5CF6]/45 hover:text-white"
            aria-label={name}
          >
            <SocialIcon name={name} />
          </a>
        ))}
      </div>

      <button
        type="button"
        className="mt-3.5 inline-flex h-9 w-full items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] text-[13px] font-bold text-white shadow-[0_0_18px_rgba(139,92,246,0.35)] transition hover:brightness-110"
      >
        دنبال کردن
      </button>
    </aside>
  );
}

export function ArticleTagsCard({ tags }: { tags: string[] }) {
  if (tags.length === 0) return null;
  return (
    <section className="rounded-xl border border-white/[0.08] bg-[#080D1F]/85 p-4 backdrop-blur-xl">
      <h2 className="mb-3 text-[13px] font-extrabold text-white">برچسب‌ها</h2>
      <div className="flex flex-wrap gap-1.5">
        {tags.map((tag) => (
          <Link
            key={tag}
            href={`/search?q=${encodeURIComponent(tag)}`}
            className="rounded-full border border-white/[0.08] bg-white/[0.03] px-2.5 py-1 text-[11.5px] font-semibold text-[#CBD5E1] no-underline transition hover:border-[#8B5CF6]/4 hover:text-white"
          >
            #{tag}
          </Link>
        ))}
      </div>
    </section>
  );
}
