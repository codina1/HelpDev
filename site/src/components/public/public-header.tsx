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
 * Public glass header — 72px, RTL, logo at start (right), nav centered.
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
      <header className="pub-navbar" style={{ minHeight: "var(--home-header-height)" }}>
        <PublicContainer size="wide" className="pub-navbar-inner">
          <Link href="/" className="pub-navbar-brand focus-ring">
            <span className="pub-navbar-mark" aria-hidden>
              H
            </span>
            <span className="pub-navbar-wordmark">{SITE.name}</span>
          </Link>

          <nav className="pub-navbar-nav" aria-label="ناوبری اصلی">
            {HEADER_NAV.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className="pub-navbar-link focus-ring"
                aria-current={isActive(item.href) ? "page" : undefined}
              >
                {item.label}
              </Link>
            ))}
          </nav>

          <div className="pub-navbar-actions">
            <button
              type="button"
              onClick={() => setPaletteOpen(true)}
              className="pub-navbar-ai focus-ring"
              aria-label="از AI بپرس"
            >
              <SparkleIcon />
              <span className="hidden sm:inline">از AI بپرس</span>
            </button>

            <IconButton
              className="pub-navbar-search"
              label="جستجوی AI — Ctrl+K"
              onClick={() => setPaletteOpen(true)}
            >
              <SearchIcon />
            </IconButton>

            <ThemeToggle />

            {isReady && user ? (
              <>
                <Link href="/dashboard" className="pub-navbar-quiet focus-ring hidden md:inline-flex">
                  داشبورد
                </Link>
                <Link
                  href="/profile"
                  className="pub-navbar-avatar focus-ring"
                  aria-label={getUserDisplayName(user)}
                >
                  <UserAvatar user={user} />
                </Link>
                <button
                  type="button"
                  onClick={logout}
                  className="pub-navbar-quiet focus-ring hidden lg:inline-flex"
                >
                  خروج
                </button>
              </>
            ) : (
              <button
                type="button"
                onClick={() => setAuthOpen(true)}
                className="pub-navbar-login focus-ring"
              >
                ورود
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
          <div className="pub-navbar-menu lg:hidden">
            <nav
              className="mx-auto flex w-full max-w-[var(--home-container-wide)] flex-col px-4 py-2"
              aria-label="ناوبری موبایل"
            >
              {HEADER_NAV.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className="pub-navbar-link focus-ring"
                  aria-current={isActive(item.href) ? "page" : undefined}
                >
                  {item.label}
                </Link>
              ))}
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
      className={["pub-navbar-icon focus-ring", className].join(" ")}
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
  }, []);

  function toggle() {
    const next = !dark;
    setDark(next);
    document.documentElement.classList.toggle("dark", next);
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
      <img src={user.profileImageUrl} alt="" className="h-6 w-6 rounded-full object-cover" />
    );
  }
  return (
    <span className="flex h-6 w-6 items-center justify-center rounded-full bg-[color:var(--home-purple-soft)] text-[10px] font-semibold text-[color:var(--home-text)]">
      {getUserInitials(user)}
    </span>
  );
}

function SparkleIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2.5 13.8 8.2 19.5 10 13.8 11.8 12 17.5 10.2 11.8 4.5 10 10.2 8.2 12 2.5Z" />
    </svg>
  );
}

function SearchIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3-3" />
    </svg>
  );
}

function MoonIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <path d="M21 14.5A8.5 8.5 0 1 1 9.5 3 7 7 0 0 0 21 14.5Z" />
    </svg>
  );
}

function SunIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      <circle cx="12" cy="12" r="4" />
      <path d="M12 3v2M12 19v2M3 12h2M19 12h2M5.6 5.6l1.4 1.4M17 17l1.4 1.4M18.4 5.6 17 7M7 17l-1.4 1.4" />
    </svg>
  );
}

function MenuIcon({ open }: { open: boolean }) {
  return (
    <svg width="15" height="15" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      {open ? <path d="M6 6l12 12M18 6L6 18" /> : <path d="M4 8h16M4 16h16" />}
    </svg>
  );
}
