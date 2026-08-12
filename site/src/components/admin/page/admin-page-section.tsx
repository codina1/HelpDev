import type { ReactNode } from "react";

type AdminPageSectionProps = {
  title?: string;
  description?: string;
  actions?: ReactNode;
  children: ReactNode;
  className?: string;
};

/** A titled section within an Admin page, with optional actions. */
export function AdminPageSection({
  title,
  description,
  actions,
  children,
  className = "",
}: AdminPageSectionProps) {
  return (
    <section className={`space-y-3 ${className}`.trim()}>
      {title || actions ? (
        <div className="flex flex-wrap items-center justify-between gap-2">
          <div className="space-y-0.5">
            {title ? (
              <h2 className="adm-text text-[15px] font-bold">{title}</h2>
            ) : null}
            {description ? (
              <p className="adm-muted text-[12px]">{description}</p>
            ) : null}
          </div>
          {actions ? <div className="flex items-center gap-2">{actions}</div> : null}
        </div>
      ) : null}
      {children}
    </section>
  );
}
