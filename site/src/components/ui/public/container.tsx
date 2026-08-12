type ContainerProps = {
  children: React.ReactNode;
  className?: string;
  as?: "div" | "section" | "article" | "main";
  size?: "default" | "narrow" | "wide" | "full";
};

const SIZE_CLASS = {
  default: "max-w-[1400px]",
  narrow: "max-w-3xl",
  wide: "max-w-[1600px]",
  full: "max-w-none",
} as const;

/**
 * Horizontal page gutter shared by public marketing surfaces.
 * Uses design tokens so dark/light themes stay compatible.
 */
export function Container({
  children,
  className = "",
  as: Tag = "div",
  size = "default",
}: ContainerProps) {
  return (
    <Tag
      className={[
        "mx-auto w-full px-4 sm:px-5 lg:px-6",
        SIZE_CLASS[size],
        className,
      ].join(" ")}
    >
      {children}
    </Tag>
  );
}
