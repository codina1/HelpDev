/**
 * Prompt Lab page shell — wide desktop content (~1180–1240px).
 */
export function PromptLabContainer({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      className={[
        "mx-auto w-[calc(100%-32px)] max-w-[1220px] md:w-[calc(100%-48px)]",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
