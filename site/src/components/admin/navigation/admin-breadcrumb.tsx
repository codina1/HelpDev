import Link from "next/link";
import type { AdminBreadcrumb as AdminBreadcrumbItem } from "@/lib/admin/breadcrumbs";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

export type AdminBreadcrumbProps = {
  items: AdminBreadcrumbItem[];
};

/** Accessible RTL breadcrumb trail. The current crumb is not a link. */
export function AdminBreadcrumb({ items }: AdminBreadcrumbProps) {
  if (items.length === 0) return null;

  return (
    <nav aria-label="مسیر" className="min-w-0">
      <ol className="flex flex-wrap items-center gap-1 text-[12px]">
        {items.map((item, index) => {
          const isLast = index === items.length - 1;
          return (
            <li key={`${item.title}-${index}`} className="flex items-center gap-1">
              {index > 0 ? (
                <AdminIcon
                  name="collapse"
                  size={13}
                  className="adm-subtle"
                />
              ) : null}
              {item.href && !item.current ? (
                <Link
                  href={item.href}
                  className="adm-focus adm-subtle rounded px-1 hover:text-[var(--adm-text)]"
                >
                  {item.title}
                </Link>
              ) : (
                <span
                  className={
                    isLast
                      ? "adm-text max-w-[40vw] truncate font-semibold"
                      : "adm-subtle"
                  }
                  aria-current={isLast ? "page" : undefined}
                >
                  {item.title}
                </span>
              )}
            </li>
          );
        })}
      </ol>
    </nav>
  );
}
