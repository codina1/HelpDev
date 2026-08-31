/** Page shell for the roadmap page — matches the courses catalog width. */
export function RoadmapContainer({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={["mx-auto w-full max-w-[1440px] px-4 sm:px-6 lg:px-8", className].join(" ")}>
      {children}
    </div>
  );
}
