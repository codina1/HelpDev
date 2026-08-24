"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState, type ReactNode } from "react";
import { AuthModal, useAuth } from "@/components/auth";
import { GlobalSearchPalette } from "@/components/search/global-search-palette";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PUBLIC_PRODUCTS_NAV } from "@/lib/public/nav-v2";
import { SITE } from "@/lib/constants";
import { getUserDisplayName, getUserInitials } from "@/types/auth";

const HEADER_NAV = [{ href: "/", label: "خانه" }, ...PUBLIC_PRODUCTS_NAV] as const;
const THEME_STORAGE_KEY = "helpdev-public-theme";

/**
 * Public sticky header — 72px, glass blur, RTL.
 * Desktop: Logo | Nav | Search + Theme + Auth
 * Mobile: Logo | Search | Menu
 */
export function PublicHeader() {
  const pathname = usePathname();
  const { user, logout, isReady } = useAuth();
  const [authOpen, setAuthOpen] = useState(false);
  const [paletteOpen, setPaletteOpen] = useState(false);
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setPaletteOpen(true);
      }
    }
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, []);

  useEffect(() => {
    setMenuOpen(false);
  }, [pathname]);

  function isActive(href: string) {
    if (href === "/") return pathname === "/";
    return pathname === href || pathname.startsWith(`${href}/`);
  }

  return (
    <>
      <header
        className="pub-navbar sticky top-0 z-50 border-b border-white/[0.08] bg-[#050816]/70 backdrop-blur-xl backdrop-saturate-150"
        style={{ minHeight: "var(--home-header-height)" }}
      >
        <PublicContainer size="wide" className="pub-navbar-inner h-[72px]">
          {/* Start (right in RTL): Logo */}
          <Link
            href="/"
            className="pub-navbar-brand focus-ring group inline-flex min-w-0 items-center gap-2.5 no-underline"
          >
            <span
              className="pub-navbar-mark flex h-7 w-7 shrink-0 items-center justify-center rounded-lg bg-gradient-to-br from-[#7C3AED] via-[#6366F1] to-[#06B6D4] text-[11px] font-extrabold text-white shadow-[0_0_20px_rgba(124,58,237,0.35)] transition group-hover:shadow-[0_0_28px_rgba(124,58,237,0.5)]"
              aria-hidden
            >
              H
            </span>
            <span className="pub-navbar-wordmark text-[15px] font-semibold tracking-tight text-white">
              {SITE.name}
            </span>
          </Link>

          {/* Center: Desktop nav */}
          <nav
            className="pub-navbar-nav hidden items-center gap-0.5 lg:flex"
            aria-label="ناوبری اصلی"
          >
            {HEADER_NAV.map((item) => {
              const active = isActive(item.href);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={[
                    "pub-navbar-link focus-ring relative rounded-md px-3 py-1.5 text-[13px] font-medium no-underline transition-colors",
                    active
                      ? "text-white"
                      : "text-[#94A3B8] hover:text-white",
                  ].join(" ")}
                  aria-current={active ? "page" : undefined}
                >
                  {item.label}
                  {active ? (
                    <span
                      className="pointer-events-none absolute inset-x-2 -bottom-px h-px bg-gradient-to-l from-transparent via-[#7C3AED] to-transparent shadow-[0_0_12px_2px_rgba(124,58,237,0.55)]"
                      aria-hidden
                    />
                  ) : null}
                </Link>
              );
            })}
          </nav>

          {/* End (left in RTL): actions */}
          <div className="pub-navbar-actions flex min-w-0 items-center justify-end gap-1.5">
            <IconButton
              className="pub-navbar-search"
              label="جستجو — Ctrl+K"
              onClick={() => setPaletteOpen(true)}
            >
              <SearchIcon />
            </IconButton>

            <div className="hidden items-center gap-1.5 sm:flex">
              <ThemeToggle />
            </div>

            {isReady && user ? (
              <div className="hidden items-center gap-1.5 md:flex">
                <Link
                  href="/dashboard"
                  className="pub-navbar-quiet focus-ring hidden rounded-md px-2.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] no-underline transition hover:bg-white/[0.04] hover:text-white lg:inline-flex"
                >
                  داشبورد
                </Link>
                <Link
                  href="/profile"
                  className="pub-navbar-avatar focus-ring inline-flex h-8 w-8 items-center justify-center overflow-hidden rounded-full ring-1 ring-white/10"
                  aria-label={getUserDisplayName(user)}
                >
                  <UserAvatar user={user} />
                </Link>
                <button
                  type="button"
                  onClick={logout}
                  className="pub-navbar-quiet focus-ring hidden rounded-md px-2.5 py-1.5 text-[12px] font-semibold text-[#94A3B8] transition hover:bg-white/[0.04] hover:text-white xl:inline-flex"
                >
                  خروج
                </button>
              </div>
            ) : (
              <button
                type="button"
                onClick={() => setAuthOpen(true)}
                className="pub-navbar-login focus-ring hidden h-8 items-center justify-center rounded-lg bg-[#7C3AED] px-3.5 text-[12px] font-semibold text-white shadow-[0_0_20px_rgba(124,58,237,0.35)] transition hover:bg-[#6D28D9] hover:shadow-[0_0_28px_rgba(124,58,237,0.5)] sm:inline-flex"
              >
                ورود / ثبت‌نام
              </button>
            )}

            <IconButton
              className="lg:hidden"
              label={menuOpen ? "بستن منو" : "باز کردن منو"}
              pressed={menuOpen}
              onClick={() => setMenuOpen((open) => !open)}
            >
              <MenuIcon open={menuOpen} />
            </IconButton>
          </div>
        </PublicContainer>

        {menuOpen ? (
          <div className="pub-navbar-menu border-t border-white/[0.08] bg-[#050816]/92 backdrop-blur-xl lg:hidden">
            <nav
              className="mx-auto flex w-full max-w-[var(--home-container-wide)] flex-col gap-0.5 px-4 py-3"
              aria-label="ناوبری موبایل"
            >
              {HEADER_NAV.map((item) => {
                const active = isActive(item.href);
                return (
                  <Link
                    key={item.href}
                    href={item.href}
                    className={[
                      "focus-ring rounded-lg px-3 py-2.5 text-[14px] font-medium no-underline transition",
                      active
                        ? "bg-[rgba(124,58,237,0.14)] text-white shadow-[inset_0_0_0_1px_rgba(124,58,237,0.35)]"
                        : "text-[#94A3B8] hover:bg-white/[0.04] hover:text-white",
                    ].join(" ")}
                    aria-current={active ? "page" : undefined}
                  >
                    {item.label}
                  </Link>
                );
              })}

              <div className="mt-2 flex items-center gap-2 border-t border-white/[0.08] pt-3 sm:hidden">
                <ThemeToggle />
                {isReady && user ? (
                  <>
                    <Link
                      href="/dashboard"
                      className="focus-ring flex-1 rounded-lg px-3 py-2 text-center text-[13px] font-semibold text-[#94A3B8] no-underline hover:bg-white/[0.04] hover:text-white"
                    >
                      داشبورد
                    </Link>
                    <button
                      type="button"
                      onClick={logout}
                      className="focus-ring rounded-lg px-3 py-2 text-[13px] font-semibold text-[#94A3B8] hover:bg-white/[0.04] hover:text-white"
                    >
                      خروج
                    </button>
                  </>
                ) : (
                  <button
                    type="button"
                    onClick={() => {
                      setMenuOpen(false);
                      setAuthOpen(true);
                    }}
                    className="focus-ring flex-1 rounded-lg bg-[#7C3AED] px-3 py-2.5 text-[13px] font-semibold text-white shadow-[0_0_20px_rgba(124,58,237,0.35)]"
                  >
                    ورود / ثبت‌نام
                  </button>
                )}
              </div>
            </nav>
          </div>
        ) : null}
      </header>

      <GlobalSearchPalette open={paletteOpen} onOpenChange={setPaletteOpen} />
      <AuthModal open={authOpen} onClose={() => setAuthOpen(false)} />
    </>
  );
}

function IconButton({
  label,
  onClick,
  children,
  className = "",
  pressed,
}: {
  label: string;
  onClick: () => void;
  children: ReactNode;
  className?: string;
  pressed?: boolean;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      aria-label={label}
      aria-pressed={pressed}
      className={[
        "pub-navbar-icon focus-ring inline-flex h-8 w-8 items-center justify-center rounded-lg border-0 bg-transparent text-[#94A3B8] transition hover:bg-white/[0.06] hover:text-white",
        className,
      ].join(" ")}
    >
      {children}
    </button>
  );
}

function ThemeToggle() {
  const [dark, setDark] = useState(true);

  useEffect(() => {
    const stored = window.localStorage.getItem(THEME_STORAGE_KEY);
    const isDark = stored !== "light";
    setDark(isDark);
    document.documentElement.classList.toggle("dark", isDark);
    document.documentElement.style.colorScheme = isDark ? "dark" : "light";
  }, []);

  function toggle() {
    const next = !dark;
    setDark(next);
    document.documentElement.classList.toggle("dark", next);
    document.documentElement.style.colorScheme = next ? "dark" : "light";
    window.localStorage.setItem(THEME_STORAGE_KEY, next ? "dark" : "light");
  }

  return (
    <IconButton label={dark ? "تم روشن" : "تم تیره"} pressed={dark} onClick={toggle}>
      {dark ? <MoonIcon /> : <SunIcon />}
    </IconButton>
  );
}

function UserAvatar({ user }: { user: NonNullable<ReturnType<typeof useAuth>["user"]> }) {
  if (user.profileImageUrl) {
    return (
      // eslint-disable-next-line @next/next/no-img-element
      <img src={user.profileImageUrl} alt="" className="h-full w-full object-cover" />
    );
  }
  return (
    <span className="flex h-full w-full items-center justify-center bg-[rgba(124,58,237,0.2)] text-[10px] font-semibold text-white">
      {getUserInitials(user)}
    </span>
  );
}

function SearchIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3-3" />
    </svg>
  );
}

function MoonIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <path d="M21 14.5A8.5 8.5 0 1 1 9.5 3 7 7 0 0 0 21 14.5Z" />
    </svg>
  );
}

function SunIcon() {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <circle cx="12" cy="12" r="4" />
      <path d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M18.4 5.6 17 7M7 17l-1.4 1.4" />
    </svg>
  );
}

function MenuIcon({ open }: { open: boolean }) {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      {open ? <path d="M6 6l12 12M18 6L6 18" /> : <path d="M4 8h16M4 16h16" />}
    </svg>
  );
}
