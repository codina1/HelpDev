type GlassCardProps = {
  children: React.ReactNode;
  className?: string;
  as?: "div" | "article" | "section";
  elevate?: boolean;
  gradientBorder?: boolean;
  strong?: boolean;
};

export function GlassCard({
  children,
  className = "",
  as: Tag = "div",
  elevate = true,
  gradientBorder = false,
  strong = false,
}: GlassCardProps) {
  return (
    <Tag
      className={[
        "rounded-[var(--pub-radius)]",
        strong ? "pub-glass-strong" : "pub-glass",
        elevate ? "pub-card-elevate" : "",
        gradientBorder ? "pub-border-gradient" : "",
        className,
      ].join(" ")}
    >
      <div className="relative z-[1] h-full">{children}</div>
    </Tag>
  );
}
