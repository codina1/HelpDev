"use client";

type ToolsPaginationProps = {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

function toFa(value: number): string {
  return value.toLocaleString("fa-IR", { useGrouping: false });
}

function pageItems(page: number, totalPages: number): Array<number | "ellipsis"> {
  if (totalPages <= 7) {
    return Array.from({ length: totalPages }, (_, index) => index + 1);
  }

  const items: Array<number | "ellipsis"> = [1];
  const start = Math.max(2, page - 1);
  const end = Math.min(totalPages - 1, page + 1);

  if (start > 2) items.push("ellipsis");
  for (let value = start; value <= end; value += 1) items.push(value);
  if (end < totalPages - 1) items.push("ellipsis");
  items.push(totalPages);
  return items;
}

/** Centered numbered pagination — active purple square. */
export function ToolsPagination({ page, totalPages, onPageChange }: ToolsPaginationProps) {
  if (totalPages <= 1) return null;
  const items = pageItems(page, totalPages);

  return (
    <nav className="mt-6 flex items-center justify-center gap-2" aria-label="صفحه‌بندی ابزارها">
      <button
        type="button"
        disabled={page <= 1}
        onClick={() => onPageChange(page - 1)}
        aria-label="صفحه قبل"
        className="inline-flex h-8 w-8 items-center justify-center rounded-lg border border-white/[0.1] bg-[#0F1626] text-[#94A3B8] transition hover:text-white disabled:opacity-40"
      >
        <svg className="h-4 w-4" viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M10 6.5 15.5 12 10 17.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      </button>

      {items.map((item, index) =>
        item === "ellipsis" ? (
          <span key={`e-${index}`} className="px-1 text-[13px] text-[#64748B]">
            …
          </span>
        ) : (
          <button
            key={item}
            type="button"
            aria-current={item === page ? "page" : undefined}
            onClick={() => onPageChange(item)}
            className={[
              "inline-flex h-8 min-w-8 items-center justify-center rounded-lg px-2 text-[13px] font-bold transition",
              item === page
                ? "bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] text-white shadow-[0_0_14px_rgba(124,58,237,0.35)]"
                : "border border-white/[0.1] bg-[#0F1626] text-[#94A3B8] hover:text-white",
            ].join(" ")}
          >
            {toFa(item)}
          </button>
        ),
      )}

      <button
        type="button"
        disabled={page >= totalPages}
        onClick={() => onPageChange(page + 1)}
        aria-label="صفحه بعد"
        className="inline-flex h-8 w-8 items-center justify-center rounded-lg border border-white/[0.1] bg-[#0F1626] text-[#94A3B8] transition hover:text-white disabled:opacity-40"
      >
        <svg className="h-4 w-4" viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M14 6.5 8.5 12l5.5 5.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      </button>
    </nav>
  );
}
