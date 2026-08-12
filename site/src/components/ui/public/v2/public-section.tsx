import { PublicContainer } from "@/components/ui/public/v2/public-container";

type PublicSectionProps = {
  children: React.ReactNode;
  id?: string;
  className?: string;
  containerClassName?: string;
  containerSize?: "default" | "narrow" | "wide" | "full";
  bare?: boolean;
  "aria-labelledby"?: string;
  "aria-label"?: string;
};

export function PublicSection({
  children,
  id,
  className = "",
  containerClassName = "",
  containerSize = "default",
  bare = false,
  "aria-labelledby": ariaLabelledBy,
  "aria-label": ariaLabel,
}: PublicSectionProps) {
  return (
    <section
      id={id}
      aria-labelledby={ariaLabelledBy}
      aria-label={ariaLabel}
      className={["relative py-12 sm:py-14 lg:py-20", className].join(" ")}
    >
      {bare ? (
        children
      ) : (
        <PublicContainer size={containerSize} className={containerClassName}>
          {children}
        </PublicContainer>
      )}
    </section>
  );
}
