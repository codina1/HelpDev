import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeTrustMark, type HomeTrustMarkItem } from "@/components/public/home/home-trust-mark";

/** Local stack marks only — not a customer roster or partnership claim. */
export const HOME_TRUST_MARKS: readonly HomeTrustMarkItem[] = [
  { id: "next", name: "Next.js", accent: "purple" },
  { id: "vercel", name: "Vercel", accent: "blue" },
  { id: "dotnet", name: ".NET", accent: "cyan" },
  { id: "postgres", name: "PostgreSQL", accent: "purple" },
  { id: "github", name: "GitHub", accent: "blue" },
];

/**
 * Homepage stack strip — glass tiles with local marks.
 */
export function HomeTrustSection() {
  return (
    <PublicSection
      className="home-trust home-reveal"
      containerSize="wide"
      aria-labelledby="home-trust-heading"
    >
      <div className="home-trust-panel">
        <div className="home-trust-head text-start">
          <h2 id="home-trust-heading" className="home-section-title">
            مورد اعتماد تیم‌های حرفه‌ای
          </h2>
          <p className="home-trust-lead">
            فریم‌ورک‌ها و ابزارهایی که پلتفرم با آن‌ها ساخته شده است — نه فهرست مشتری.
          </p>
        </div>
        <ul className="home-trust-row" aria-label="نشان‌های پشته فناوری">
          {HOME_TRUST_MARKS.map((item) => (
            <HomeTrustMark key={item.id} item={item} />
          ))}
        </ul>
      </div>
    </PublicSection>
  );
}
