type MediaSkeletonProps = {
  count?: number;
};

/** Loading placeholder grid — mirrors the real card aspect ratio to avoid layout jumps. */
export function MediaSkeleton({ count = 12 }: MediaSkeletonProps) {
  return (
    <div
      className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6"
      role="status"
      aria-live="polite"
    >
      <span className="sr-only">در حال بارگذاری رسانه‌ها...</span>
      {Array.from({ length: count }).map((_, index) => (
        <div key={index} className="adm-surface space-y-2 rounded-xl p-2">
          <div className="adm-skeleton aspect-square w-full rounded-lg" />
          <div className="adm-skeleton h-3 w-4/5" />
          <div className="adm-skeleton h-2.5 w-2/5" />
        </div>
      ))}
    </div>
  );
}
