/**
 * Page shell for the roadmap page.
 * Reference layout: content spans `calc(100% - 48px)` up to ~1136px on desktop.
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
        "mx-auto w-[calc(100%-32px)] max-w-[1136px] md:w-[calc(100%-48px)]",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
