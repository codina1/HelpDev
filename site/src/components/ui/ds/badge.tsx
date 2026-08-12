export type DsBadgeVariant = "default" | "primary" | "secondary" | "ai" | "success" | "warning" | "outline";

type BadgeProps = {
  children: React.ReactNode;
  variant?: DsBadgeVariant;
  className?: string;
};

const VARIANT: Record<DsBadgeVariant, string> = {
  default: "border-[color:var(--ds-border)] bg-white/[0.04] text-[color:var(--ds-fg)]",
  primary:
    "border-[color:color-mix(in_srgb,var(--ds-primary)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-primary)_14%,transparent)] text-[color:var(--ds-primary)]",
  secondary:
    "border-[color:color-mix(in_srgb,var(--ds-secondary)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-secondary)_12%,transparent)] text-[color:var(--ds-secondary)]",
  ai: "border-[color:color-mix(in_srgb,var(--ds-primary)_45%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-primary)_16%,transparent)] text-[#a78bfa] shadow-[var(--ds-shadow-glow)]",
  success:
    "border-[color:color-mix(in_srgb,var(--ds-success)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-success)_12%,transparent)] text-[color:var(--ds-success)]",
  warning:
    "border-[color:color-mix(in_srgb,var(--ds-warning)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-warning)_12%,transparent)] text-[color:var(--ds-warning)]",
  outline: "border-[color:var(--ds-border-strong)] bg-transparent text-[color:var(--ds-muted)]",
};

export function Badge({ children, variant = "default", className = "" }: BadgeProps) {
  return (
    <span
      className={[
        "inline-flex items-center gap-1 rounded-[var(--ds-radius-sm)] border px-2 py-0.5 text-[10px] font-bold leading-none sm:text-[11px]",
        VARIANT[variant],
        className,
      ].join(" ")}
    >
      {children}
    </span>
  );
}
