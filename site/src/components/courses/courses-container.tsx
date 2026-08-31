/** Fixed page shell for the courses catalog — matches the news page width. */
export function CoursesContainer({
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
