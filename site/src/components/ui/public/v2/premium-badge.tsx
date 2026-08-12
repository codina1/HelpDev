export type PremiumBadgeVariant = "ai" | "primary" | "cyan" | "muted" | "success" | "outline";

type PremiumBadgeProps = {
  children: React.ReactNode;
  variant?: PremiumBadgeVariant;
  className?: string;
};

const VARIANT: Record<PremiumBadgeVariant, string> = {
  ai: "border-[color:color-mix(in_srgb,var(--pub-ai-from)_45%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-primary)_16%,transparent)] text-[color:var(--pub-ai-from)] shadow-[0_0_16px_var(--pub-glow)]",
  primary:
    "border-[color:color-mix(in_srgb,var(--pub-primary)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-primary)_14%,transparent)] text-[color:var(--pub-primary)]",
  cyan: "border-[color:color-mix(in_srgb,var(--pub-secondary)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--pub-secondary)_12%,transparent)] text-[color:var(--pub-secondary)]",
  muted: "border-[color:var(--pub-glass-border)] bg-white/[0.03] text-[color:var(--pub-muted)]",
  success: "border-emerald-400/35 bg-emerald-500/12 text-emerald-300",
  outline: "border-[color:var(--pub-glass-border)] bg-transparent text-[color:var(--pub-muted)]",
};

export function PremiumBadge({ children, variant = "primary", className = "" }: PremiumBadgeProps) {
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
