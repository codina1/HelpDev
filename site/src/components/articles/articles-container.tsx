/**
 * Shared articles / news detail shell.
 * ~95vw with 1400px cap — aligned with Roadmap / header visual width.
 */
export function ArticlesContainer({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={["mx-auto w-[95%] max-w-[1400px] min-w-0", className].join(" ")}>
      {children}
    </div>
  );
}
