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
 * Premium SaaS navbar — 64px, glass blur, RTL, Linear/Vercel density.
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
        className="pub-navbar sticky top-0 z-50 border-b border-white/[0.08] bg-[#050816]/80 backdrop-blur-xl"
        style={{ minHeight: "var(--home-header-height)" }}
      >
        <PublicContainer size="wide" className="pub-navbar-inner h-[64px]">
          {/* Right (RTL start): compact logo */}
          <Link
            href="/"
            className="pub-navbar-brand focus-ring group inline-flex min-w-0 items-center gap-2 no-underline"
          >
            <span
              className="pub-navbar-mark relative flex h-7 w-7 shrink-0 items-center justify-center text-[#A855F7] drop-shadow-[0_0_12px_rgba(168,85,247,0.55)]"
              aria-hidden
            >
              <BrandMarkIcon />
            </span>
            <span className="pub-navbar-wordmark text-[14px] font-semibold tracking-tight text-white">
              {SITE.name}
            </span>
          </Link>

          {/* Center: sparse nav */}
          <nav
            className="pub-navbar-nav hidden items-center gap-0.5 sm:flex lg:gap-1 xl:gap-2"
            aria-label="ناوبری اصلی"
          >
            {HEADER_NAV.map((item) => {
              const active = isActive(item.href);
              return (
                <Link
                  key={item.href}
                  href={item.href}
                  className={[
                    "pub-navbar-link focus-ring relative px-1.5 py-1.5 text-[11px] font-medium tracking-wide no-underline transition-colors lg:px-2.5 lg:text-[12px] xl:px-3",
                    active ? "text-white" : "text-[#94A3B8] hover:text-white",
                  ].join(" ")}
                  aria-current={active ? "page" : undefined}
                >
                  {item.label}
                  {active ? (
                    <span
                      className="pointer-events-none absolute inset-x-1 -bottom-0.5 h-px rounded-full bg-[#7C3AED] shadow-[0_0_10px_3px_rgba(124,58,237,0.65)] lg:inset-x-1.5"
                      aria-hidden
                    />
                  ) : null}
                </Link>
              );
            })}
          </nav>

          {/* Left (RTL end): search box · theme · auth */}
          <div className="pub-navbar-actions flex min-w-0 items-center justify-end gap-1.5 sm:gap-2">
            <button
              type="button"
              onClick={() => setPaletteOpen(true)}
              className="pub-navbar-search focus-ring hidden h-9 min-w-[180px] max-w-[260px] flex-1 items-center gap-2 rounded-xl border border-white/[0.1] bg-[#0B1224]/90 px-3 text-start transition hover:border-[rgba(168,85,247,0.4)] hover:bg-[#111827] md:inline-flex lg:min-w-[220px]"
              aria-label="جستجو — Ctrl+K"
            >
              <span className="text-[#94A3B8]">
                <SearchIcon />
              </span>
              <span className="truncate text-[12px] font-medium text-[#64748B]">جستجو کنید...</span>
              <kbd className="ms-auto hidden rounded-md border border-white/[0.08] bg-white/[0.04] px-1.5 py-0.5 text-[10px] font-semibold text-[#64748B] lg:inline">
                ⌘K
              </kbd>
            </button>

            <IconButton
              className="pub-navbar-search md:hidden"
              label="جستجو — Ctrl+K"
              onClick={() => setPaletteOpen(true)}
            >
              <SearchIcon />
            </IconButton>

            <div className="hidden sm:block">
              <ThemeToggle />
            </div>

            {isReady && user ? (
              <div className="hidden items-center gap-1 md:flex">
                <Link
                  href="/dashboard"
                  className="focus-ring hidden rounded-md px-2 py-1.5 text-[11px] font-semibold text-[#94A3B8] no-underline transition hover:text-white lg:inline-flex"
                >
                  داشبورد
                </Link>
                <Link
                  href="/profile"
                  className="focus-ring inline-flex h-7 w-7 items-center justify-center overflow-hidden rounded-full ring-1 ring-white/10"
                  aria-label={getUserDisplayName(user)}
                >
                  <UserAvatar user={user} />
                </Link>
                <button
                  type="button"
                  onClick={logout}
                  className="focus-ring hidden rounded-md px-2 py-1.5 text-[11px] font-semibold text-[#94A3B8] transition hover:text-white xl:inline-flex"
                >
                  خروج
                </button>
              </div>
            ) : (
              <button
                type="button"
                onClick={() => setAuthOpen(true)}
                className="pub-navbar-login focus-ring hidden h-9 items-center justify-center rounded-xl bg-gradient-to-l from-[#7C3AED] to-[#6D28D9] px-4 text-[12px] font-semibold text-white shadow-[0_0_18px_rgba(124,58,237,0.35)] transition hover:brightness-110 sm:inline-flex"
              >
                ورود / ثبت‌نام
              </button>
            )}

            <IconButton
              className="sm:hidden"
              label={menuOpen ? "بستن منو" : "باز کردن منو"}
              pressed={menuOpen}
              onClick={() => setMenuOpen((open) => !open)}
            >
              <MenuIcon open={menuOpen} />
            </IconButton>
          </div>
        </PublicContainer>

        {menuOpen ? (
          <div className="border-t border-white/[0.08] bg-[#050816]/95 backdrop-blur-xl sm:hidden">
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
                      "focus-ring rounded-lg px-3 py-2.5 text-[13px] font-medium no-underline transition",
                      active
                        ? "bg-[rgba(124,58,237,0.14)] text-white"
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
                      className="focus-ring flex-1 rounded-lg px-3 py-2 text-center text-[12px] font-semibold text-[#94A3B8] no-underline"
                    >
                      داشبورد
                    </Link>
                    <button
                      type="button"
                      onClick={logout}
                      className="focus-ring rounded-lg px-3 py-2 text-[12px] font-semibold text-[#94A3B8]"
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
                    className="focus-ring flex-1 rounded-xl bg-[#7C3AED] px-3 py-2.5 text-[12px] font-semibold text-white"
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
        "focus-ring inline-flex h-8 w-8 items-center justify-center rounded-lg text-[#94A3B8] transition hover:bg-white/[0.06] hover:text-white",
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
    <span className="flex h-full w-full items-center justify-center bg-[rgba(124,58,237,0.2)] text-[9px] font-semibold text-white">
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

/** Stylized loop / heart mark — matches reference brand glyph. */
function BrandMarkIcon() {
  return (
    <svg width="26" height="26" viewBox="0 0 28 28" fill="none" aria-hidden>
      <defs>
        <linearGradient id="hd-brand" x1="4" y1="4" x2="24" y2="24" gradientUnits="userSpaceOnUse">
          <stop stopColor="#C084FC" />
          <stop offset="0.55" stopColor="#A855F7" />
          <stop offset="1" stopColor="#7C3AED" />
        </linearGradient>
      </defs>
      <path
        d="M14 23.2c-1.1-.7-6.6-4.4-8.8-8.2C3.2 11.4 4 7.4 7.1 5.8c2-.9 4.1-.3 5.4 1.3C13.8 5.5 15.9 4.9 17.9 5.8c3.1 1.6 3.9 5.6 1.9 9.2-2.2 3.8-7.7 7.5-8.8 8.2Z"
        fill="url(#hd-brand)"
        opacity="0.95"
      />
      <path
        d="M11.2 11.4c.7-1.4 2.1-2.1 3.5-1.8 1.6.3 2.7 1.7 2.7 3.4 0 2.2-1.8 3.6-3.4 4.8"
        stroke="#F5F3FF"
        strokeWidth="1.5"
        strokeLinecap="round"
        fill="none"
        opacity="0.85"
      />
    </svg>
  );
}
