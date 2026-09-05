import Link from "next/link";

type Crumb = { label: string; href?: string };

export function ArticleBreadcrumb({ items }: { items: Crumb[] }) {
  return (
    <nav aria-label="مسیر صفحه" className="text-[12px] leading-5 text-[#94A3B8]" dir="rtl">
      <ol className="flex flex-wrap items-center gap-1.5">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          const isHome = index === 0;
          return (
            <li key={`${item.label}-${index}`} className="inline-flex min-w-0 items-center gap-1.5">
              {index > 0 ? <span className="text-[#64748B]" aria-hidden>›</span> : null}
              {item.href && !isLast ? (
                <Link
                  href={item.href}
                  className="inline-flex items-center gap-1 rounded transition hover:text-[#C4B5FD]"
                >
                  {isHome ? (
                    <svg viewBox="0 0 24 24" className="h-3.5 w-3.5" fill="none" aria-hidden>
                      <path
                        d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6H10v6H5a1 1 0 0 1-1-1v-9.5Z"
                        stroke="currentColor"
                        strokeWidth="1.6"
                        strokeLinejoin="round"
                      />
                    </svg>
                  ) : null}
                  <span className="truncate">{item.label}</span>
                </Link>
              ) : (
                <span className={`truncate ${isLast ? "text-[#CBD5E1]" : ""}`}>{item.label}</span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
