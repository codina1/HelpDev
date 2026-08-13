const NUMBER_FA = new Intl.NumberFormat("fa-IR");

function formatStatValue(value: number): string {
  if (!Number.isFinite(value)) return "۰";
  return NUMBER_FA.format(value);
}

export const HOME_STAT_ICONS = [
  "engineers",
  "articles",
  "paths",
  "tools",
  "questions",
] as const;

export type HomeStatIcon = (typeof HOME_STAT_ICONS)[number];

export type HomeStatItem = {
  id: string;
  label: string;
  value: number;
  icon: HomeStatIcon;
};

type HomeStatProps = {
  item: HomeStatItem;
  separated?: boolean;
};

/** Single homepage statistic — icon, Persian value, label. */
export function HomeStat({ item, separated = false }: HomeStatProps) {
  return (
    <div
      className={[
        "home-stat flex min-w-0 flex-1 basis-[calc(50%-1px)] items-center gap-2.5 px-3 py-3.5 sm:min-w-[10.5rem] sm:basis-auto sm:gap-3 sm:px-5 sm:py-4",
        separated
          ? "border-t border-[color:var(--home-border)] sm:border-t-0 sm:border-s"
          : "",
      ].join(" ")}
    >
      <span
        className="flex h-9 w-9 shrink-0 items-center justify-center rounded-[var(--home-radius-md)]"
        style={{
          background: "var(--home-purple-soft)",
          color: "var(--home-purple)",
        }}
        aria-hidden
      >
        <StatIcon name={item.icon} />
      </span>
      <div className="min-w-0 text-start">
        <p
          className="text-[1.35rem] font-extrabold tabular-nums leading-none tracking-tight text-[color:var(--home-text)] sm:text-[1.5rem]"
        >
          {formatStatValue(item.value)}
        </p>
        <p className="mt-1.5 truncate text-[12px] font-medium text-[color:var(--home-text-muted)]">
          {item.label}
        </p>
      </div>
    </div>
  );
}

function StatIcon({ name }: { name: HomeStatIcon }) {
  const common = {
    width: 16,
    height: 16,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.75,
  } as const;

  if (name === "engineers") {
    return (
      <svg {...common} aria-hidden>
        <path d="M16 21v-2a4 4 0 0 0-4-4H6a4 4 0 0 0-4 4v2" />
        <circle cx="9" cy="7" r="3" />
        <path d="M22 21v-2a4 4 0 0 0-3-3.87M16 3.13a4 4 0 0 1 0 7.75" />
      </svg>
    );
  }
  if (name === "articles") {
    return (
      <svg {...common} aria-hidden>
        <path d="M7 3h8l5 5v13H7z" />
        <path d="M15 3v5h5M10 13h7M10 17h5" />
      </svg>
    );
  }
  if (name === "paths") {
    return (
      <svg {...common} aria-hidden>
        <circle cx="6" cy="6" r="2.2" />
        <circle cx="18" cy="12" r="2.2" />
        <circle cx="8" cy="18" r="2.2" />
        <path d="M8 7.5 16 11M16.5 14 9.5 17" />
      </svg>
    );
  }
  if (name === "tools") {
    return (
      <svg {...common} aria-hidden>
        <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
      </svg>
    );
  }
  return (
    <svg {...common} aria-hidden>
      <path d="M21 12a8.5 8.5 0 0 1-11.6 8L3 21l1.2-6.2A8.5 8.5 0 1 1 21 12Z" />
    </svg>
  );
}
