type PublicContainerProps = {
  children: React.ReactNode;
  className?: string;
  as?: "div" | "section" | "article" | "main";
  size?: "default" | "narrow" | "wide" | "full";
};

const SIZE = {
  default: "max-w-[1280px]",
  narrow: "max-w-3xl",
  wide: "max-w-[1280px]",
  full: "max-w-none",
} as const;

/** Shared page shell — 1280 max · pad 24 / tablet 20 / desktop 32. */
export function PublicContainer({
  children,
  className = "",
  as: Tag = "div",
  size = "default",
}: PublicContainerProps) {
  return (
    <Tag
      className={[
        "mx-auto w-full min-w-0 px-6 sm:px-5 lg:px-6 min-[1440px]:px-8",
        SIZE[size],
        className,
      ].join(" ")}
    >
      {children}
    </Tag>
  );
}
