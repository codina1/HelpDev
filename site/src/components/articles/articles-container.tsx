/** Articles marketplace page shell — ~1200px content width. */
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
        "mx-auto w-[calc(100%-32px)] max-w-[1200px] md:w-[calc(100%-48px)]",
        className,
      ].join(" ")}
    >
      {children}
    </div>
  );
}
