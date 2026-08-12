type GradientHeadingProps = {
  children: React.ReactNode;
  as?: "h1" | "h2" | "h3";
  id?: string;
  className?: string;
  /** Supporting line under the heading */
  subtitle?: string;
  /** `hero` uses a brighter white→violet gradient for landing heroes. */
  tone?: "brand" | "hero";
};

/**
 * Premium gradient title for public hero / section heads.
 */
export function GradientHeading({
  children,
  as: Tag = "h2",
  id,
  className = "",
  subtitle,
  tone = "brand",
}: GradientHeadingProps) {
  const gradient =
    tone === "hero"
      ? "bg-gradient-to-l from-white via-violet-100 to-indigo-200 bg-clip-text text-transparent"
      : "bg-gradient-to-l from-[color:var(--accent)] via-violet-300 to-[color:var(--accent-2)] bg-clip-text text-transparent";

  return (
    <div className={className}>
      <Tag
        id={id}
        className={[
          gradient,
          Tag === "h1"
            ? "text-3xl font-extrabold tracking-tight sm:text-4xl lg:text-5xl"
            : Tag === "h2"
              ? "text-xl font-extrabold tracking-tight sm:text-2xl lg:text-3xl"
              : "text-lg font-bold sm:text-xl",
        ].join(" ")}
      >
        {children}
      </Tag>
      {subtitle ? (
        <p className="mt-2 max-w-2xl text-sm leading-relaxed text-[color:var(--muted)] sm:text-[15px]">
          {subtitle}
        </p>
      ) : null}
    </div>
  );
}
