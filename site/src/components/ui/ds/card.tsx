export type DsCardVariant = "default" | "glass" | "elevated" | "outline";

type CardProps = {
  children: React.ReactNode;
  className?: string;
  variant?: DsCardVariant;
  hover?: boolean;
  as?: "div" | "article" | "section";
};

const VARIANT: Record<DsCardVariant, string> = {
  default: "bg-[color:var(--ds-surface)] border-[color:var(--ds-border)]",
  glass:
    "bg-[color:color-mix(in_srgb,var(--ds-surface)_70%,transparent)] border-[color:var(--ds-border-strong)] backdrop-blur-xl",
  elevated: "bg-[color:var(--ds-surface-elevated)] border-[color:var(--ds-border-strong)] shadow-[var(--ds-shadow-md)]",
  outline: "bg-transparent border-[color:var(--ds-border-strong)]",
};

export function Card({
  children,
  className = "",
  variant = "glass",
  hover = true,
  as: Tag = "div",
}: CardProps) {
  return (
    <Tag
      className={[
        "rounded-[var(--ds-radius-xl)] border p-4 shadow-[var(--ds-shadow-sm)]",
        VARIANT[variant],
        hover ? "ds-hover-lift" : "",
        className,
      ].join(" ")}
    >
      {children}
    </Tag>
  );
}
