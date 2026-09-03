import type { MarketplaceTool } from "@/data/tools";

function ToolLogo({ name }: { name: string }) {
  const cls = "h-10 w-10";
  switch (name) {
    case "chatgpt":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3.2c2.1 0 3.9 1.2 4.8 3 .8-.3 1.7-.3 2.5.1 1.7.9 2.5 3 1.8 4.8.8 1 1.1 2.3.7 3.5-.7 2-2.7 3.2-4.8 3-.9 1.8-2.7 3-4.8 3s-3.9-1.2-4.8-3c-2.1.2-4.1-1-4.8-3-.4-1.2-.1-2.5.7-3.5-.7-1.8.1-3.9 1.8-4.8.8-.4 1.7-.4 2.5-.1.9-1.8 2.7-3 4.8-3Z" stroke="#22D3EE" strokeWidth="1.4" />
          <circle cx="12" cy="12" r="2.2" fill="#22D3EE" />
        </svg>
      );
    case "github":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="#E5E7EB" aria-hidden>
          <path d="M12 2.4A9.6 9.6 0 0 0 2.4 12c0 4.2 2.7 7.8 6.5 9.1.5.1.7-.2.7-.5v-1.8c-2.6.6-3.2-1.1-3.2-1.1-.4-1.1-1-1.4-1-1.4-.9-.6.1-.6.1-.6 1 .1 1.5 1 1.5 1 .9 1.5 2.3 1.1 2.9.8.1-.7.3-1.1.6-1.3-2.1-.2-4.4-1.1-4.4-4.7 0-1 .4-1.9 1-2.6-.1-.2-.4-1.3.1-2.6 0 0 .8-.3 2.7 1a9 9 0 0 1 4.9 0c1.9-1.3 2.7-1 2.7-1 .5 1.3.2 2.4.1 2.6.6.7 1 1.6 1 2.6 0 3.6-2.2 4.5-4.4 4.7.4.3.7.9.7 1.8v2.7c0 .3.2.6.7.5A9.6 9.6 0 0 0 21.6 12 9.6 9.6 0 0 0 12 2.4Z" />
        </svg>
      );
    case "vercel":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="#FFFFFF" aria-hidden>
          <path d="M12 4.5 21 19.5H3L12 4.5Z" />
        </svg>
      );
    case "figma":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="9" cy="7" r="3" fill="#F24E1E" />
          <circle cx="15" cy="7" r="3" fill="#FF7262" />
          <circle cx="9" cy="12" r="3" fill="#A259FF" />
          <circle cx="15" cy="12" r="3" fill="#1ABCFE" />
          <circle cx="9" cy="17" r="3" fill="#0ACF83" />
        </svg>
      );
    case "vscode":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M4 7.5 10.5 4l9.5 4.5v7L10.5 20 4 16.5v-9Z" stroke="#3B82F6" strokeWidth="1.5" strokeLinejoin="round" />
          <path d="M10.5 4v16M4 7.5l6.5 4 9.5-3" stroke="#60A5FA" strokeWidth="1.5" strokeLinejoin="round" />
        </svg>
      );
    case "postman":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="8" stroke="#F97316" strokeWidth="1.6" />
          <path d="M8 14.5 16 9.5 13 16.5 8 14.5Z" fill="#F97316" />
        </svg>
      );
    case "docker":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M3.5 13.5h14.2c2.4 0 3.8-1.3 3.8-3.1 0-1.4-.9-2.4-2.4-2.8-.4-2.2-2-3.6-4.3-3.6-1.2 0-2.3.4-3.1 1.2C10.7 4.3 9.4 3.8 8 3.8c-2.1 0-3.8 1.5-4.1 3.5C2.5 7.7 1.5 9 1.5 10.6c0 1.6 1 2.9 2 2.9Z" stroke="#38BDF8" strokeWidth="1.4" />
          <path d="M6 11V9.2h1.8V11H6Zm2.6 0V9.2h1.8V11H8.6Zm2.6 0V9.2H13V11h-1.8Zm2.6 0V9.2h1.8V11H13.8Z" fill="#38BDF8" />
        </svg>
      );
    case "mongodb":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3.5c2.4 3 4 6.2 4 9.2 0 3.4-1.5 5.8-4 7.8-2.5-2-4-4.4-4-7.8 0-3 1.6-6.2 4-9.2Z" stroke="#22C55E" strokeWidth="1.6" strokeLinejoin="round" />
          <path d="M12 5.5v13" stroke="#22C55E" strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "tailwind":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M6.5 12c1.3-4 3-6 5.5-6s3.3 1.3 4 4c.7 2.7 1.7 4 3 4s2.3-1.3 3-4c-1.3 4-3 6-5.5 6s-3.3-1.3-4-4c-.7-2.7-1.7-4-3-4s-2.3 1.3-3 4Z" stroke="#22D3EE" strokeWidth="1.5" strokeLinejoin="round" />
        </svg>
      );
    case "netlify":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3.5 20.5 12 12 20.5 3.5 12 12 3.5Z" stroke="#14B8A6" strokeWidth="1.5" strokeLinejoin="round" />
          <path d="M12 8.5v7M8.5 12h7" stroke="#14B8A6" strokeWidth="1.5" strokeLinecap="round" />
        </svg>
      );
    case "prisma":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M7 18.5 11.2 4.5h2.4L17 19.5l-4.2-2.2L7 18.5Z" stroke="#A5B4FC" strokeWidth="1.5" strokeLinejoin="round" />
        </svg>
      );
    case "firebase":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M6.5 17.5 10 5.5l2.2 4.2L6.5 17.5Zm0 0L12 20.5l5.5-3L13.5 7.8 12 9.7 6.5 17.5Z" stroke="#FBBF24" strokeWidth="1.4" strokeLinejoin="round" />
        </svg>
      );
    case "linear":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M6 16.5 16.5 6M6 11.5 11.5 6M6 18.5 18.5 6" stroke="#A78BFA" strokeWidth="1.8" strokeLinecap="round" />
        </svg>
      );
    case "snyk":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M12 3.5 5.5 6.2v5.2c0 4.2 2.8 7.5 6.5 8.8 3.7-1.3 6.5-4.6 6.5-8.8V6.2L12 3.5Z" stroke="#A855F7" strokeWidth="1.6" strokeLinejoin="round" />
          <path d="M9.5 12.2 11.2 14l3.4-3.8" stroke="#A855F7" strokeWidth="1.6" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
    case "supabase":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8 19.5V8.2L16.5 19.5H8Zm0 0h8.5L12 4.5 8 19.5Z" stroke="#34D399" strokeWidth="1.5" strokeLinejoin="round" />
        </svg>
      );
    case "cursor":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M5.5 4.5 18.5 12 5.5 19.5V4.5Z" stroke="#E5E7EB" strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    default:
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="5" y="5" width="14" height="14" rx="3" stroke="#A78BFA" strokeWidth="1.6" />
        </svg>
      );
  }
}

type ToolCardProps = {
  tool: MarketplaceTool;
};

/** Glass marketplace tool card — logo · title · rating · hover CTA. */
export function ToolCard({ tool }: ToolCardProps) {
  return (
    <article
      className="group relative flex h-[270px] min-w-0 flex-col overflow-hidden rounded-[16px] border border-white/[0.08] bg-[#111827]/90 p-4 pt-5 shadow-[0_4px_16px_rgba(2,6,23,0.25)] backdrop-blur-sm transition duration-300 hover:-translate-y-1 hover:border-[rgba(168,85,247,0.45)] hover:shadow-[0_0_30px_rgba(124,58,237,0.25)]"
      dir="rtl"
    >
      {/* Logo area — larger with ambient glow */}
      <div className="relative mx-auto flex h-[88px] w-[88px] items-center justify-center rounded-2xl border border-white/[0.08] bg-[radial-gradient(circle_at_center,rgba(124,58,237,0.22),rgba(15,23,42,0.9)_70%)] shadow-[0_0_28px_rgba(124,58,237,0.25)]">
        <span
          className="pointer-events-none absolute inset-[-20%] rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.18),transparent_65%)] blur-xl"
          aria-hidden
        />
        <span className="relative scale-[1.2]">
          <ToolLogo name={tool.logo} />
        </span>
      </div>

      <div className="mt-3 min-w-0 flex-1 text-center">
        <h3 className="truncate text-[15px] font-bold text-white">{tool.name}</h3>
        <p className="mt-1.5 line-clamp-2 text-[12px] leading-[1.8] text-[#94A3B8]">{tool.description}</p>
      </div>

      <div className="mt-auto flex items-center justify-between gap-2 border-t border-white/[0.06] pt-3 text-[11.5px] font-semibold">
        <span className="inline-flex items-center rounded-md bg-white/[0.05] px-2 py-1 text-[#CBD5E1]">
          {tool.categoryLabel}
        </span>
        <span className="inline-flex items-center gap-1 text-[#FBBF24]">
          <svg className="h-3.5 w-3.5" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
            <path d="M12 3.4 14.4 9l6 .5-4.6 3.9 1.4 5.8L12 16.8 6.8 19.2l1.4-5.8L3.6 9.5l6-.5L12 3.4Z" />
          </svg>
          <bdi>{tool.rating.toFixed(1)}</bdi>
          <span className="text-[#64748B]">({tool.reviewCount.toLocaleString("fa-IR")})</span>
        </span>
      </div>

      <a
        href={tool.href}
        target="_blank"
        rel="noopener noreferrer"
        className="focus-ring pointer-events-none absolute inset-x-4 bottom-4 z-10 inline-flex h-9 translate-y-2 items-center justify-center rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] text-[12.5px] font-bold text-white no-underline opacity-0 shadow-[0_0_16px_rgba(124,58,237,0.35)] transition duration-300 group-hover:pointer-events-auto group-hover:translate-y-0 group-hover:opacity-100"
      >
        مشاهده ابزار
      </a>
    </article>
  );
}
