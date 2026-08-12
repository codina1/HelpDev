type AdminLoadingStateProps = {
  /** Number of skeleton stat cards to render. */
  cards?: number;
  /** Number of skeleton table rows to render. */
  rows?: number;
  label?: string;
};

/**
 * Content-area loading skeleton. The sidebar and header remain stable; only the
 * page content is replaced, avoiding a full-page spinner after auth resolves.
 */
export function AdminLoadingState({
  cards = 4,
  rows = 5,
  label = "در حال بارگذاری...",
}: AdminLoadingStateProps) {
  return (
    <div className="space-y-5" role="status" aria-live="polite">
      <span className="sr-only">{label}</span>

      <div className="adm-skeleton h-8 w-56" />

      {cards > 0 ? (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-4">
          {Array.from({ length: cards }).map((_, index) => (
            <div key={index} className="adm-surface rounded-xl p-4">
              <div className="adm-skeleton mb-3 h-3 w-20" />
              <div className="adm-skeleton h-7 w-24" />
            </div>
          ))}
        </div>
      ) : null}

      {rows > 0 ? (
        <div className="adm-surface rounded-xl p-4">
          <div className="space-y-3">
            {Array.from({ length: rows }).map((_, index) => (
              <div key={index} className="adm-skeleton h-10 w-full" />
            ))}
          </div>
        </div>
      ) : null}
    </div>
  );
}
