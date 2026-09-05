import { PublicContainer } from "@/components/ui/public/v2/public-container";

/** Shared articles shell — same width as header / footer (PublicContainer wide). */
export function ArticlesContainer({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <PublicContainer size="wide" className={className}>
      {children}
    </PublicContainer>
  );
}
