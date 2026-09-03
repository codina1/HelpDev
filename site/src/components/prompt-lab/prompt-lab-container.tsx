/**
 * Prompt Lab page shell — reference content width ~1200px.
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
        "mx-auto w-[calc(100%-32px)] max-w-[1200px] md:w-[calc(100%-48px)]",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
