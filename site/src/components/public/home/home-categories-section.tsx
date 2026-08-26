import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PublicSection } from "@/components/ui/public/v2/public-section";

export const HOME_CONTENT_CATEGORIES = [
  { id: "ai", label: "AI Coding", href: "/search?q=AI%20Coding", icon: "/home/icon-ai.png" },
  { id: "dotnet", label: ".NET", href: "/search?q=.NET", icon: "/home/icon-dotnet.png" },
  { id: "frontend", label: "Frontend", href: "/search?q=Frontend", icon: "/home/icon-frontend.png" },
  { id: "backend", label: "Backend", href: "/search?q=Backend", icon: "/home/icon-backend.png" },
  { id: "devops", label: "DevOps", href: "/search?q=DevOps", icon: "/home/icon-devops.png" },
  { id: "mobile", label: "Mobile", href: "/search?q=Mobile", icon: "/home/icon-mobile.png" },
  { id: "database", label: "Database", href: "/search?q=Database", icon: "/home/icon-database.png" },
  { id: "security", label: "Security", href: "/search?q=Security", icon: "/home/icon-security.png" },
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
                  className="flex h-11 w-11 items-center justify-center drop-shadow-[0_8px_18px_rgba(124,58,237,0.35)]"
                  aria-hidden
                >
                  <img
                    src={category.icon}
                    alt=""
                    width={44}
                    height={44}
                    decoding="async"
                    className="h-11 w-11 object-contain"
                  />
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
