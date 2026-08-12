export type PublicBadgeVariant =
  | "default"
  | "accent"
  | "success"
  | "warning"
  | "ai"
  | "muted"
  | "outline";

type PublicBadgeProps = {
  children: React.ReactNode;
  variant?: PublicBadgeVariant;
  className?: string;
};

const VARIANT: Record<PublicBadgeVariant, string> = {
  default:
    "border-[color:var(--border-strong)] bg-[color:var(--surface-elevated)] text-[color:var(--foreground)]",
  accent:
    "border-[color:color-mix(in_srgb,var(--accent)_40%,transparent)] bg-[color:var(--accent-soft)] text-[color:var(--accent)]",
  success:
    "border-emerald-500/35 bg-emerald-500/12 text-emerald-300",
  warning:
    "border-amber-500/35 bg-amber-500/12 text-amber-300",
  ai:
    "border-fuchsia-500/35 bg-fuchsia-500/12 text-fuchsia-300",
  muted:
    "border-[color:var(--border)] bg-transparent text-[color:var(--muted)]",
  outline:
    "border-[color:var(--border-strong)] bg-transparent text-[color:var(--muted)]",
};

/**
 * Compact public-surface badge. Token-based for dark/light compatibility.
 */
export function Badge({
  children,
  variant = "default",
  className = "",
}: PublicBadgeProps) {
  return (
    <span
      className={[
        "inline-flex items-center gap-1 rounded-md border px-2 py-0.5 text-[10px] font-bold leading-none sm:text-[11px]",
        VARIANT[variant],
        className,
      ].join(" ")}
    >
      {children}
    </span>
  );
}
