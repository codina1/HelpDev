import Link from "next/link";
import { coverForHomePath } from "@/lib/public/home-covers";

export const HOME_PATH_VISUALS = [
  "architect",
  "frontend",
  "devops",
  "ai",
  "backend",
] as const;

export type HomePathVisual = (typeof HOME_PATH_VISUALS)[number];

export type HomePathItem = {
  id: string;
  title: string;
  description: string;
  href: string;
  learners: number;
  visual: HomePathVisual;
};

type HomePathCardProps = {
  item: HomePathItem;
};

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

function formatLearners(value: number): string {
  if (!Number.isFinite(value)) return "۰";
  return NUMBER_FA.format(value);
}

/** Horizontal learning-path card — visual, title, copy, learner count. */
export function HomePathCard({ item }: HomePathCardProps) {
  return (
    <li className="home-path-item min-w-0">
      <Link href={item.href} className={`home-path-card home-path-card-${item.visual} focus-ring`}>
        <div className="home-path-visual">
          <img src={coverForHomePath(item.visual)} alt="" className="home-path-image" />
          <span className="home-path-visual-shade" aria-hidden />
          <PathGlyph name={item.visual} />
        </div>
        <div className="home-path-body">
          <h3 className="home-path-title">{item.title}</h3>
          <p className="home-path-copy">{item.description}</p>
          <p className="home-path-learners">
            <LearnersIcon />
            <span>
              {formatLearners(item.learners)} یادگیرنده
            </span>
          </p>
        </div>
      </Link>
    </li>
  );
}

function PathGlyph({ name }: { name: HomePathVisual }) {
  const common = {
    width: 28,
    height: 28,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.6,
  } as const;

  if (name === "architect") {
    return (
      <svg className="home-path-glyph" {...common}>
        <path d="M3 20h18M5 20V9l7-5 7 5v11M10 20v-6h4v6" />
      </svg>
    );
  }
  if (name === "frontend") {
    return (
      <svg className="home-path-glyph" {...common}>
        <rect x="3" y="4" width="18" height="14" rx="2" />
        <path d="M3 8h18M8 12h5M8 15h3" />
      </svg>
    );
  }
  if (name === "devops") {
    return (
      <svg className="home-path-glyph" {...common}>
        <path d="M4 12a6 6 0 0 1 9.5-4.8L16 5" />
        <path d="M20 12a6 6 0 0 1-9.5 4.8L8 19" />
        <path d="M16 5h4v4M8 19H4v-4" />
      </svg>
    );
  }
  if (name === "ai") {
    return (
      <svg className="home-path-glyph" {...common}>
        <path d="M12 3l1.5 5.5L19 10l-5.5 1.5L12 17l-1.5-5.5L5 10l5.5-1.5L12 3z" />
      </svg>
    );
  }
  return (
    <svg className="home-path-glyph" {...common}>
      <rect x="4" y="4" width="7" height="7" rx="1.2" />
      <rect x="13" y="4" width="7" height="7" rx="1.2" />
      <rect x="4" y="13" width="7" height="7" rx="1.2" />
      <path d="M16.5 13.5v6.5M13.5 17h6.5" />
    </svg>
  );
}

function LearnersIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
      <circle cx="9" cy="7" r="3" />
      <path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
    </svg>
  );
}
