import Link from "next/link";

type Crumb = { label: string; href?: string };

type ArticleBreadcrumbProps = {
  items: Crumb[];
};

/** Compact gray breadcrumb under sticky header. */
export function ArticleBreadcrumb({ items }: ArticleBreadcrumbProps) {
  return (
    <nav aria-label="مسیر صفحه" className="text-[12px] leading-5 text-[#94A3B8]" dir="rtl">
      <ol className="flex flex-wrap items-center gap-1.5">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          return (
            <li key={`${item.label}-${index}`} className="inline-flex min-w-0 items-center gap-1.5">
              {index > 0 ? (
                <span className="text-[#64748B]" aria-hidden>
                  ›
                </span>
              ) : null}
              {item.href && !isLast ? (
                <Link
                  href={item.href}
                  className="focus-ring rounded truncate transition hover:text-[#C4B5FD]"
                >
                  {item.label}
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
