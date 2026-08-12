type GradientTextProps = {
  children: React.ReactNode;
  as?: "span" | "h1" | "h2" | "h3" | "p";
  id?: string;
  className?: string;
  animated?: boolean;
};

export function GradientText({
  children,
  as: Tag = "span",
  id,
  className = "",
  animated = false,
}: GradientTextProps) {
  return (
    <Tag
      id={id}
      className={[
        "bg-gradient-to-l from-[color:var(--pub-ai-from)] via-[color:var(--pub-ai-via)] to-[color:var(--pub-ai-to)] bg-clip-text text-transparent",
        animated ? "pub-gradient-shift" : "",
        className,
      ].join(" ")}
    >
      {children}
    </Tag>
  );
}
