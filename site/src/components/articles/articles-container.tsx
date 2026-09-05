/** Articles page shell — ~90–94% viewport · ~40px side margins. */
export function ArticlesContainer({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div
      className={[
        "mx-auto w-full max-w-[1480px] px-[36px] sm:px-[40px]",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
