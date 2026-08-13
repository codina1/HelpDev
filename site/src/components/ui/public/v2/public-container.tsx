type PublicContainerProps = {
  children: React.ReactNode;
  className?: string;
  as?: "div" | "section" | "article" | "main";
  size?: "default" | "narrow" | "wide" | "full";
};

const SIZE = {
  default: "max-w-none",
  narrow: "max-w-3xl",
  wide: "max-w-none",
  full: "max-w-none",
} as const;

export function PublicContainer({
  children,
  className = "",
  as: Tag = "div",
  size = "default",
}: PublicContainerProps) {
  return (
    <Tag className={["mx-auto w-full min-w-0 px-4 sm:px-6 lg:px-8", SIZE[size], className].join(" ")}>
      {children}
    </Tag>
  );
}
