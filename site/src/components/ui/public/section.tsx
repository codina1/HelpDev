import { Container } from "@/components/ui/public/container";

type SectionProps = {
  children: React.ReactNode;
  id?: string;
  className?: string;
  containerClassName?: string;
  containerSize?: "default" | "narrow" | "wide" | "full";
  /** When true, skips the inner Container (caller owns layout). */
  bare?: boolean;
  "aria-labelledby"?: string;
  "aria-label"?: string;
};

/**
 * Vertical rhythm + optional Container for public page sections.
 */
export function Section({
  children,
  id,
  className = "",
  containerClassName = "",
  containerSize = "default",
  bare = false,
  "aria-labelledby": ariaLabelledBy,
  "aria-label": ariaLabel,
}: SectionProps) {
  return (
    <section
      id={id}
      aria-labelledby={ariaLabelledBy}
      aria-label={ariaLabel}
      className={["py-10 sm:py-12 lg:py-16", className].join(" ")}
    >
      {bare ? (
        children
      ) : (
        <Container size={containerSize} className={containerClassName}>
          {children}
        </Container>
      )}
    </section>
  );
}
