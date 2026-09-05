type PublicContainerProps = {
  children: React.ReactNode;
  className?: string;
  as?: "div" | "section" | "article" | "main";
  size?: "default" | "narrow" | "wide" | "full";
};

const SIZE = {
  default: "max-w-[1400px]",
  narrow: "max-w-3xl",
  wide: "max-w-[1400px]",
  full: "max-w-none",
} as const;

/** Shared page shell — ~1400 max · ~40px side padding (≈94% on common desktops). */
export function PublicContainer({
  children,
  className = "",
  as: Tag = "div",
  size = "default",
}: PublicContainerProps) {
  return (
    <Tag
      className={[
        "mx-auto w-full min-w-0 px-9 sm:px-10",
        SIZE[size],
        className,
      ].join(" ")}
    >
      {children}
    </Tag>
  );
}
