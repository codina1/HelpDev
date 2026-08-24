import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";

export const HOME_CONTENT_CATEGORIES = [
  { id: "ai", label: "AI Coding", href: "/search?q=AI%20Coding" },
  { id: "dotnet", label: ".NET", href: "/search?q=.NET" },
  { id: "frontend", label: "Frontend", href: "/search?q=Frontend" },
  { id: "backend", label: "Backend", href: "/search?q=Backend" },
  { id: "devops", label: "DevOps", href: "/search?q=DevOps" },
  { id: "mobile", label: "Mobile", href: "/search?q=Mobile" },
  { id: "database", label: "Database", href: "/search?q=Database" },
  { id: "security", label: "Security", href: "/search?q=Security" },
] as const;

/** Content category chips / cards grid. */
export function HomeCategoriesSection() {
  return (
    <PublicSection
      className="home-categories home-reveal"
      bare
      aria-labelledby="home-categories-heading"
    >
      <PublicContainer size="wide">
        <div className="mb-5 text-start sm:mb-6">
          <p className="text-[12px] font-bold tracking-wide text-[#06B6D4]">دسته‌بندی</p>
          <h2
            id="home-categories-heading"
            className="mt-1 text-[1.25rem] font-extrabold text-white sm:text-[1.4rem]"
          >
            دسته‌بندی مطالب
          </h2>
        </div>

        <ul className="grid grid-cols-2 gap-3 sm:grid-cols-4 lg:gap-4">
          {HOME_CONTENT_CATEGORIES.map((category) => (
            <li key={category.id}>
              <Link
                href={category.href}
                className="focus-ring flex min-h-[88px] flex-col items-center justify-center gap-2 rounded-2xl border border-white/[0.08] bg-[#0B1224] px-3 py-4 text-center no-underline transition hover:-translate-y-1 hover:border-[rgba(124,58,237,0.4)] hover:shadow-[0_0_28px_rgba(124,58,237,0.2)]"
              >
                <span
                  className="flex h-10 w-10 items-center justify-center rounded-xl bg-[rgba(124,58,237,0.16)] text-[#C4B5FD]"
                  aria-hidden
                >
                  <CategoryIcon id={category.id} />
                </span>
                <span className="text-[13px] font-bold text-white">{category.label}</span>
              </Link>
            </li>
          ))}
        </ul>
      </PublicContainer>
    </PublicSection>
  );
}

function CategoryIcon({ id }: { id: string }) {
  const common = {
    width: 18,
    height: 18,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.7,
  } as const;

  if (id === "ai") {
    return (
      <svg {...common} aria-hidden>
        <path d="M12 3 13.8 8.2 19 10 13.8 11.8 12 17 10.2 11.8 5 10 10.2 8.2 12 3Z" />
      </svg>
    );
  }
  if (id === "frontend" || id === "mobile") {
    return (
      <svg {...common} aria-hidden>
        <rect x="4" y="5" width="16" height="12" rx="2" />
        <path d="M8 21h8" />
      </svg>
    );
  }
  if (id === "security") {
    return (
      <svg {...common} aria-hidden>
        <path d="M12 3 19 6v5c0 5-3.2 8.2-7 9-3.8-.8-7-4-7-9V6l7-3Z" />
      </svg>
    );
  }
  return (
    <svg {...common} aria-hidden>
      <path d="M4 7h16M4 12h16M4 17h10" />
    </svg>
  );
}
