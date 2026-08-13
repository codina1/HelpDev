import Link from "next/link";
import { coverForHomeValue } from "@/lib/public/home-covers";

export const HOME_VALUE_ICONS = ["paths", "tools", "ai", "knowledge"] as const;

export type HomeValueIcon = (typeof HOME_VALUE_ICONS)[number];

export type HomeValueAccent = "purple" | "cyan" | "ai" | "blue";

export type HomeValueItem = {
  id: string;
  title: string;
  description: string;
  href: string;
  icon: HomeValueIcon;
  accent: HomeValueAccent;
};

type HomeValueCardProps = {
  item: HomeValueItem;
};

/** Single value-proposition card — icon, title, description, glow. */
export function HomeValueCard({ item }: HomeValueCardProps) {
  return (
    <li>
      <Link
        href={item.href}
        className={`home-value-card home-value-card-${item.accent} focus-ring`}
      >
        <div className="home-value-visual">
          <img src={coverForHomeValue(item.id)} alt="" className="home-value-image" />
        </div>
        <span className="home-value-icon" aria-hidden>
          <ValueIcon name={item.icon} />
        </span>
        <h3 className="home-value-title">{item.title}</h3>
        <p className="home-value-copy">{item.description}</p>
      </Link>
    </li>
  );
}

function ValueIcon({ name }: { name: HomeValueIcon }) {
  const common = {
    width: 20,
    height: 20,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.75,
  } as const;

  if (name === "paths") {
    return (
      <svg {...common}>
        <circle cx="6" cy="6" r="2.2" />
        <circle cx="18" cy="12" r="2.2" />
        <circle cx="8" cy="18" r="2.2" />
        <path d="M8 7.5 16 11M16.5 14 9.5 17" />
      </svg>
    );
  }
  if (name === "tools") {
    return (
      <svg {...common}>
        <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
      </svg>
    );
  }
  if (name === "ai") {
    return (
      <svg {...common}>
        <path d="M12 3l1.5 5.5L19 10l-5.5 1.5L12 17l-1.5-5.5L5 10l5.5-1.5L12 3z" />
        <path d="M19 16.5 20 19l2.5 1L20 21l-1 2.5L18 21l-2.5-1L18 19l1-2.5Z" />
      </svg>
    );
  }
  return (
    <svg {...common}>
      <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20" />
      <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2Z" />
      <path d="M9 7h7M9 11h5" />
    </svg>
  );
}
