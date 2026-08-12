import Link from "next/link";

export type DsButtonVariant = "primary" | "secondary" | "ghost" | "danger";
export type DsButtonSize = "sm" | "md" | "lg";

type ButtonProps = {
  children: React.ReactNode;
  variant?: DsButtonVariant;
  size?: DsButtonSize;
  href?: string;
  type?: "button" | "submit" | "reset";
  disabled?: boolean;
  className?: string;
  onClick?: () => void;
  "aria-label"?: string;
};

const VARIANT: Record<DsButtonVariant, string> = {
  primary:
    "bg-gradient-to-l from-[color:var(--ds-primary)] to-[color:var(--ds-primary-strong)] text-white shadow-[var(--ds-shadow-glow)] hover:brightness-110",
  secondary:
    "border border-[color:var(--ds-border-strong)] bg-[color:var(--ds-surface)] text-[color:var(--ds-fg)] hover:border-[color:color-mix(in_srgb,var(--ds-secondary)_50%,transparent)] hover:shadow-[var(--ds-shadow-glow-cyan)]",
  ghost: "text-[color:var(--ds-muted)] hover:bg-white/5 hover:text-[color:var(--ds-fg)]",
  danger:
    "border border-[color:color-mix(in_srgb,var(--ds-danger)_40%,transparent)] bg-[color:color-mix(in_srgb,var(--ds-danger)_12%,transparent)] text-[color:var(--ds-danger)] hover:bg-[color:color-mix(in_srgb,var(--ds-danger)_20%,transparent)]",
};

const SIZE: Record<DsButtonSize, string> = {
  sm: "rounded-[var(--ds-radius-md)] px-3 py-1.5 text-[12px]",
  md: "rounded-[var(--ds-radius-lg)] px-5 py-2.5 text-[13px]",
  lg: "rounded-[var(--ds-radius-lg)] px-6 py-3 text-[14px]",
};

export function Button({
  children,
  variant = "primary",
  size = "md",
  href,
  type = "button",
  disabled = false,
  className = "",
  onClick,
  "aria-label": ariaLabel,
}: ButtonProps) {
  const classes = [
    "focus-ring ds-hover-lift inline-flex items-center justify-center gap-2 font-bold transition-all duration-300 disabled:pointer-events-none disabled:opacity-50",
    VARIANT[variant],
    SIZE[size],
    className,
  ].join(" ");

  if (href) {
    return (
      <Link href={href} className={classes} aria-label={ariaLabel} onClick={onClick}>
        {children}
      </Link>
    );
  }

  return (
    <button type={type} className={classes} disabled={disabled} onClick={onClick} aria-label={ariaLabel}>
      {children}
    </button>
  );
}
