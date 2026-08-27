/** Exact 1200px news page shell — matches reference container. */
export function NewsContainer({
  children,
  className = "",
}: {
  children: React.ReactNode;
  className?: string;
}) {
  return (
    <div className={["mx-auto w-full max-w-[1200px] px-4 sm:px-5 lg:px-6", className].join(" ")}>
      {children}
    </div>
  );
}
