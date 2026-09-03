"use client";

type PromptLabPaginationProps = {
  page: number;
  totalPages: number;
  onPageChange: (page: number) => void;
};

function toFa(value: number): string {
  return value.toLocaleString("fa-IR", { useGrouping: false });
}

function pageWindow(page: number, totalPages: number): number[] {
  const max = Math.min(totalPages, 5);
  let start = Math.max(1, page - 2);
  const end = Math.min(totalPages, start + max - 1);
  start = Math.max(1, end - max + 1);
  return Array.from({ length: end - start + 1 }, (_, index) => start + index);
}

/** Centered numbered pagination — active purple square. */
export function PromptLabPagination({ page, totalPages, onPageChange }: PromptLabPaginationProps) {
  if (totalPages <= 1) return null;
  const pages = pageWindow(page, totalPages);

  return (
    <nav className="mt-6 flex items-center justify-center gap-2" aria-label="صفحه‌بندی پرامپت‌ها">
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

      {pages.map((item) => {
        const active = item === page;
        return (
          <button
            key={item}
            type="button"
            aria-current={active ? "page" : undefined}
            onClick={() => onPageChange(item)}
            className={[
              "inline-flex h-8 min-w-8 items-center justify-center rounded-lg px-2 text-[13px] font-bold transition",
              active
                ? "bg-[#7C3AED] text-white shadow-[0_0_14px_rgba(124,58,237,0.35)]"
                : "border border-white/[0.1] bg-[#0F1626] text-[#94A3B8] hover:text-white",
            ].join(" ")}
          >
            {toFa(item)}
          </button>
        );
      })}

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
