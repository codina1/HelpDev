import Link from "next/link";
import { Button } from "@/components/ui/ds/button";

type GlowButtonProps = {
  children: React.ReactNode;
  href?: string;
  onClick?: () => void;
  type?: "button" | "submit";
  variant?: "primary" | "secondary" | "ghost";
  className?: string;
  disabled?: boolean;
  "aria-label"?: string;
};

/**
 * Public glow CTA — thin wrapper over DS Button for backward compatibility.
 */
export function GlowButton({
  children,
  href,
  onClick,
  type = "button",
  variant = "primary",
  className = "",
  disabled = false,
  "aria-label": ariaLabel,
}: GlowButtonProps) {
  return (
    <Button
      href={href}
      onClick={onClick}
      type={type}
      variant={variant}
      disabled={disabled}
      aria-label={ariaLabel}
      className={className}
    >
      {children}
    </Button>
  );
}

/** Optional direct link helper used in some public surfaces. */
export function GlowLink({
  href,
  children,
  className = "",
}: {
  href: string;
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <Link href={href} className={["focus-ring text-[color:var(--ds-primary)]", className].join(" ")}>
      {children}
    </Link>
  );
}
