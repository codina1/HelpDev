import type { ReactNode } from "react";

/**
 * Main Admin content region. Wide layout with responsive padding, sized to host
 * full-width data tables and sticky action bars without sidebar overlap.
 */
export function AdminContentArea({ children }: { children: ReactNode }) {
  return (
    <main
      id="admin-main"
      tabIndex={-1}
      className="adm-scroll flex-1 overflow-y-auto px-4 py-5 outline-none sm:px-5 lg:px-8 lg:py-7"
    >
      <div className="mx-auto w-full max-w-[1600px]">{children}</div>
    </main>
  );
}
