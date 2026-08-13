import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeTrustMark, type HomeTrustMarkItem } from "@/components/public/home/home-trust-mark";

/** Local placeholders only — stack wordmarks, not a customer roster. */
export const HOME_TRUST_MARKS: readonly HomeTrustMarkItem[] = [
  { id: "next", name: "Next.js", src: "/next.svg" },
  { id: "vercel", name: "Vercel", src: "/vercel.svg" },
  { id: "dotnet", name: ".NET" },
  { id: "postgres", name: "PostgreSQL" },
  { id: "github", name: "GitHub" },
];

/**
 * Low-emphasis professional trust strip — glass bar, monochrome marks.
 */
export function HomeTrustSection() {
  return (
    <PublicSection
      className="home-trust home-reveal !py-6 sm:!py-7"
      containerSize="wide"
      aria-labelledby="home-trust-heading"
    >
      <div className="home-trust-panel">
        <h2 id="home-trust-heading" className="home-trust-heading">
          مورد اعتماد تیم‌های حرفه‌ای
        </h2>
        <ul className="home-trust-row" aria-label="جای‌نگهدارهای بصری پشته فناوری">
          {HOME_TRUST_MARKS.map((item) => (
            <HomeTrustMark key={item.id} item={item} />
          ))}
        </ul>
      </div>
    </PublicSection>
  );
}
