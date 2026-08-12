export type BadgeVariant =
  | "new"
  | "hot"
  | "free"
  | "trending"
  | "pro"
  | "live"
  | "ai"
  | "popular"
  | "updated"
  | "level"
  | "tag";

const VARIANT_STYLES: Record<BadgeVariant, string> = {
  new: "border-emerald-500/35 bg-emerald-500/12 text-emerald-300 shadow-[0_0_12px_rgba(16,185,129,0.15)]",
  hot: "border-orange-500/35 bg-orange-500/12 text-orange-300 shadow-[0_0_12px_rgba(249,115,22,0.15)]",
  free: "border-emerald-500/30 bg-emerald-500/10 text-emerald-400",
  trending: "border-amber-500/35 bg-amber-500/12 text-amber-300 shadow-[0_0_12px_rgba(245,158,11,0.12)]",
  pro: "border-violet-500/40 bg-gradient-to-l from-violet-600/25 to-indigo-600/20 text-violet-200",
  live: "border-red-500/35 bg-red-500/12 text-red-300",
  ai: "border-fuchsia-500/35 bg-fuchsia-500/12 text-fuchsia-300",
  popular: "border-sky-500/35 bg-sky-500/12 text-sky-300",
  updated: "border-cyan-500/35 bg-cyan-500/12 text-cyan-300",
  level: "border-indigo-500/30 bg-indigo-500/10 text-indigo-300",
  tag: "border-violet-500/25 bg-violet-500/10 text-violet-300",
};

type BadgeProps = {
  children: React.ReactNode;
  variant?: BadgeVariant;
  pulse?: boolean;
  dot?: boolean;
  size?: "sm" | "md";
  className?: string;
};

const SIZE_STYLES = {
  sm: "rounded-md px-1.5 py-0.5 text-[9px] leading-none sm:text-[10px]",
  md: "rounded-full px-2.5 py-1 text-[10px] leading-tight sm:text-[11px]",
} as const;

export function Badge({
  children,
  variant = "tag",
  pulse = false,
  dot = false,
  size = "sm",
  className = "",
}: BadgeProps) {
  const showDot = dot || (size === "sm" && (variant === "live" || variant === "new"));

  return (
    <span
      className={[
        "inline-flex items-center justify-center gap-1 border font-bold",
        SIZE_STYLES[size],
        VARIANT_STYLES[variant],
        pulse ? "badge-pulse" : "",
        className,
      ].join(" ")}
    >
      {showDot && (
        <span
          className={[
            "h-1.5 w-1.5 shrink-0 rounded-full bg-current",
            pulse || variant === "live" ? "badge-dot-pulse" : "",
          ].join(" ")}
          aria-hidden
        />
      )}
      {children}
    </span>
  );
}
