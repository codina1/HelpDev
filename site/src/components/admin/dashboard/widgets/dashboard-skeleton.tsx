/** Full-layout dashboard skeleton (used as a chart fallback / initial shell). */
export function DashboardSkeleton() {
  return (
    <div className="space-y-6" role="status" aria-live="polite">
      <span className="sr-only">در حال بارگذاری داشبورد...</span>

      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-4">
        {Array.from({ length: 4 }).map((_, index) => (
          <div key={index} className="adm-surface rounded-xl p-4">
            <div className="adm-skeleton mb-3 h-3 w-20" />
            <div className="adm-skeleton h-7 w-24" />
          </div>
        ))}
      </div>

      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <div className="adm-skeleton h-64 rounded-xl" />
        <div className="adm-skeleton h-64 rounded-xl" />
      </div>
    </div>
  );
}

/** Compact single-widget skeleton for lazy chart Suspense fallbacks. */
export function WidgetChartSkeleton() {
  return (
    <div className="adm-surface rounded-xl p-4">
      <div className="adm-skeleton mb-4 h-4 w-28" />
      <div className="adm-skeleton h-40 w-full rounded-lg" />
    </div>
  );
}
