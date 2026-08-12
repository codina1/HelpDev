"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { PUBLIC_BOTTOM_NAV } from "@/lib/public/nav-v2";

/**
 * Mobile bottom navigation — Home / Search / Learning / Profile.
 */
export function PublicBottomNav() {
  const pathname = usePathname();

  return (
    <nav
      className="fixed inset-x-0 bottom-0 z-50 border-t border-[color:var(--pub-glass-border)] bg-[color:color-mix(in_srgb,var(--pub-bg)_88%,transparent)] pb-[env(safe-area-inset-bottom,0px)] backdrop-blur-xl lg:hidden"
      aria-label="ناوبری پایین"
    >
      <ul className="mx-auto grid max-w-lg grid-cols-4 gap-1 px-2 pt-1.5 pb-1.5">
        {PUBLIC_BOTTOM_NAV.map((item) => {
          const active =
            item.href === "/"
              ? pathname === "/"
              : pathname === item.href || pathname.startsWith(`${item.href}/`);

          return (
            <li key={item.href}>
              <Link
                href={item.href}
                className={[
                  "focus-ring flex flex-col items-center gap-0.5 rounded-xl px-1 py-2 text-[10px] font-bold transition",
                  active
                    ? "bg-[color:color-mix(in_srgb,var(--pub-primary)_16%,transparent)] text-[color:var(--pub-ai-from)]"
                    : "text-[color:var(--pub-muted)] hover:text-[color:var(--pub-fg)]",
                ].join(" ")}
                aria-current={active ? "page" : undefined}
              >
                <NavIcon name={item.icon} active={active} />
                {item.label}
              </Link>
            </li>
          );
        })}
      </ul>
    </nav>
  );
}

function NavIcon({
  name,
  active,
}: {
  name: (typeof PUBLIC_BOTTOM_NAV)[number]["icon"];
  active: boolean;
}) {
  const stroke = active ? "currentColor" : "currentColor";
  const common = { width: 20, height: 20, viewBox: "0 0 24 24", fill: "none", stroke, strokeWidth: 1.8, "aria-hidden": true as const };

  if (name === "home") {
    return (
      <svg {...common}>
        <path d="M4 10.5 12 4l8 6.5V20a1 1 0 0 1-1 1h-5v-6H10v6H5a1 1 0 0 1-1-1v-9.5z" />
      </svg>
    );
  }
  if (name === "search") {
    return (
      <svg {...common}>
        <circle cx="11" cy="11" r="7" />
        <path d="m20 20-3-3" />
      </svg>
    );
  }
  if (name === "learn") {
    return (
      <svg {...common}>
        <path d="M4 19.5A2.5 2.5 0 0 1 6.5 17H20" />
        <path d="M6.5 2H20v20H6.5A2.5 2.5 0 0 1 4 19.5v-15A2.5 2.5 0 0 1 6.5 2z" />
      </svg>
    );
  }
  return (
    <svg {...common}>
      <circle cx="12" cy="8" r="4" />
      <path d="M4 20c0-4 3.6-7 8-7s8 3 8 7" />
    </svg>
  );
}
