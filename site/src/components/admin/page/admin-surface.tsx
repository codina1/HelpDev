import type { ElementType, ReactNode } from "react";

type Padding = "none" | "sm" | "md" | "lg";
type Density = "comfortable" | "compact";

const PADDING: Record<Padding, string> = {
  none: "",
  sm: "p-3",
  md: "p-5",
  lg: "p-6",
};

const DENSITY: Record<Density, string> = {
  comfortable: "",
  compact: "text-[13px]",
};

type AdminSurfaceProps = {
  children: ReactNode;
  as?: ElementType;
  padding?: Padding;
  density?: Density;
  className?: string;
};

/** Consistent bordered surface used across all Admin pages. */
export function AdminSurface({
  children,
  as: Tag = "div",
  padding = "md",
  density = "comfortable",
  className = "",
}: AdminSurfaceProps) {
  return (
    <Tag
      className={`adm-surface rounded-xl ${PADDING[padding]} ${DENSITY[density]} ${className}`.trim()}
    >
      {children}
    </Tag>
  );
}
