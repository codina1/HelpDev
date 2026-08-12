"use client";

import Link from "next/link";

type InteractiveNodeProps = {
  label: string;
  description?: string;
  active?: boolean;
  className?: string;
  style?: React.CSSProperties;
  size?: "sm" | "md" | "lg";
  tone?: "center" | "orbit";
  href?: string;
  onActivate?: () => void;
  onHoverChange?: (hovered: boolean) => void;
};

const SIZE = {
  sm: "h-11 w-11 text-[10px]",
  md: "h-14 w-14 text-[11px]",
  lg: "h-16 w-16 text-[12px] sm:h-[4.5rem] sm:w-[4.5rem] sm:text-[13px]",
} as const;

const NODE_CLASS = {
  center:
    "border-[color:color-mix(in_srgb,var(--pub-secondary)_45%,transparent)] bg-gradient-to-br from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] text-white shadow-[0_0_32px_var(--pub-glow)] exp-ai-glow",
  orbit:
    "border-[color:var(--pub-glass-border)] bg-[color:var(--pub-glass-strong)] text-[color:var(--pub-fg)]",
} as const;

/**
 * Interactive knowledge-graph node — hover glow + optional navigation.
 */
export function InteractiveNode({
  label,
  description,
  active = false,
  className = "",
  style,
  size = "md",
  tone = "orbit",
  href,
  onActivate,
  onHoverChange,
}: InteractiveNodeProps) {
  const sharedClass = [
    "exp-node focus-ring flex flex-col items-center justify-center rounded-2xl border font-extrabold backdrop-blur-md",
    SIZE[size],
    NODE_CLASS[tone],
  ].join(" ");

  const aria = description ? `${label}: ${description}` : label;

  const inner = <span className="px-1 text-center leading-tight">{label}</span>;

  return (
    <div
      className={["absolute -translate-x-1/2 -translate-y-1/2", className].join(" ")}
      style={style}
      onMouseEnter={() => onHoverChange?.(true)}
      onMouseLeave={() => onHoverChange?.(false)}
      onFocus={() => onHoverChange?.(true)}
      onBlur={() => onHoverChange?.(false)}
    >
      {href ? (
        <Link
          href={href}
          data-active={active ? "true" : "false"}
          className={sharedClass}
          aria-label={aria}
          title={description}
        >
          {inner}
        </Link>
      ) : (
        <button
          type="button"
          data-active={active ? "true" : "false"}
          className={sharedClass}
          aria-label={aria}
          title={description}
          onClick={() => onActivate?.()}
        >
          {inner}
        </button>
      )}
      {active && description ? (
        <p
          role="tooltip"
          className="absolute start-1/2 top-[calc(100%+8px)] z-10 w-40 -translate-x-1/2 rounded-xl border border-[color:var(--pub-glass-border)] bg-[color:var(--pub-bg)]/95 px-2.5 py-2 text-center text-[11px] leading-5 text-[color:var(--pub-muted)] shadow-lg backdrop-blur"
        >
          {description}
          {href || onActivate ? (
            <span className="mt-1 block text-[10px] font-semibold text-[color:var(--pub-secondary)]">
              کلیک برای ورود
            </span>
          ) : null}
        </p>
      ) : null}
    </div>
  );
}
