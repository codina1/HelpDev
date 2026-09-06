/**
 * Shared shell for the public Roadmap page.
 * ~95vw with max-width aligned to site-wide PublicContainer (1400px).
 */
export function RoadmapContainer({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      className={[
        "mx-auto w-[95%] max-w-[1400px] min-w-0",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
