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
 * Homepage / public glass header — RTL, 72px, logo + nav + AI + actions + login.
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
        className="pub-navbar sticky top-0 z-50 border-b backdrop-blur-xl backdrop-saturate-150"
        style={{
          height: "auto",
          borderColor: "var(--home-border)",
          background: "color-mix(in srgb, var(--home-bg-elevated) 72%, transparent)",
          boxShadow: "0 1px 0 color-mix(in srgb, var(--home-purple) 18%, transparent), 0 12px 40px rgba(2, 6, 23, 0.35)",
        }}
      >
        <PublicContainer size="wide" className="relative flex min-h-[var(--home-header-height)] min-w-0 items-center gap-2 sm:gap-3">
          <Link
            href="/"
            className="focus-ring relative z-10 flex min-w-0 shrink-0 items-center gap-2 rounded-[var(--home-radius-sm)]"
          >
            <span
              className="flex h-7 w-7 items-center justify-center rounded-[var(--home-radius-sm)] text-[11px] font-extrabold text-[color:var(--home-text-on-accent)]"
              style={{
                background: "linear-gradient(135deg, var(--home-purple), var(--home-blue) 55%, var(--home-cyan))",
                boxShadow: "var(--home-glow-purple)",
              }}
              aria-hidden
            >
              H
            </span>
            <span className="pub-navbar-wordmark text-[14px] font-semibold tracking-tight text-[color:var(--home-text)]">
              {SITE.name}
            </span>
          </Link>

          <nav className="hidden min-w-0 flex-1 items-center justify-center gap-0.5 lg:flex" aria-label="ناوبری اصلی">
            {HEADER_NAV.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={[
                  "focus-ring rounded-[var(--home-radius-sm)] px-3 py-1.5 text-[13px] font-medium transition-colors",
                  isActive(item.href)
                    ? "text-[color:var(--home-text)]"
                    : "text-[color:var(--home-text-muted)] hover:text-[color:var(--home-text)]",
                ].join(" ")}
                aria-current={isActive(item.href) ? "page" : undefined}
              >
                {item.label}
              </Link>
            ))}
          </nav>

          <div className="relative z-10 ms-auto flex min-w-0 shrink-0 items-center gap-0.5 sm:gap-1.5">
            <button
              type="button"
              onClick={() => setPaletteOpen(true)}
              className="focus-ring inline-flex h-8 items-center gap-1.5 rounded-[var(--home-radius-md)] px-2.5 text-[12px] font-semibold text-[color:var(--home-text)] transition-colors"
              style={{
                background: "var(--home-purple-soft)",
                border: "1px solid var(--home-border-accent)",
                boxShadow: "0 0 16px color-mix(in srgb, var(--home-purple) 22%, transparent)",
              }}
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
                <Link
                  href="/dashboard"
                  className="focus-ring hidden h-8 items-center rounded-[var(--home-radius-sm)] px-2.5 text-[12px] font-medium text-[color:var(--home-text-secondary)] transition-colors hover:bg-[color:var(--home-surface-hover)] hover:text-[color:var(--home-text)] md:inline-flex"
                >
                  داشبورد
                </Link>
                <Link
                  href="/profile"
                  className="focus-ring flex h-8 w-8 items-center justify-center overflow-hidden rounded-full"
                  aria-label={getUserDisplayName(user)}
                >
                  <UserAvatar user={user} />
                </Link>
                <button
                  type="button"
                  onClick={logout}
                  className="focus-ring hidden h-8 items-center rounded-[var(--home-radius-sm)] px-2 text-[12px] font-medium text-[color:var(--home-text-subtle)] hover:text-[color:var(--home-text-muted)] lg:inline-flex"
                >
                  خروج
                </button>
              </>
            ) : (
              <button
                type="button"
                onClick={() => setAuthOpen(true)}
                className="focus-ring inline-flex h-8 items-center rounded-[var(--home-radius-md)] border px-3 text-[12px] font-semibold text-[color:var(--home-text)] transition-colors hover:bg-[color:var(--home-surface-hover)]"
                style={{ borderColor: "var(--home-border-strong)" }}
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
          <div
            className="border-t backdrop-blur-xl lg:hidden"
            style={{
              borderColor: "var(--home-border)",
              background: "color-mix(in srgb, var(--home-bg-elevated) 92%, transparent)",
            }}
          >
            <nav
              className="mx-auto flex w-full max-w-[var(--home-container-wide)] flex-col px-4 py-2"
              aria-label="ناوبری موبایل"
            >
              {HEADER_NAV.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className={[
                    "focus-ring rounded-[var(--home-radius-sm)] px-2 py-2.5 text-[13px] font-medium",
                    isActive(item.href)
                      ? "text-[color:var(--home-text)]"
                      : "text-[color:var(--home-text-muted)]",
                  ].join(" ")}
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
      className={[
        "focus-ring inline-flex h-8 w-8 items-center justify-center rounded-[var(--home-radius-sm)] text-[color:var(--home-text-muted)] transition-colors hover:bg-[color:var(--home-surface-hover)] hover:text-[color:var(--home-text)]",
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
      <img src={user.profileImageUrl} alt="" className="h-7 w-7 rounded-full object-cover" />
    );
  }
  return (
    <span className="flex h-7 w-7 items-center justify-center rounded-full bg-[color:var(--home-purple-soft)] text-[10px] font-semibold text-[color:var(--home-text)]">
      {getUserInitials(user)}
    </span>
  );
}

function SparkleIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2.5 13.8 8.2 19.5 10 13.8 11.8 12 17.5 10.2 11.8 4.5 10 10.2 8.2 12 2.5Z" />
    </svg>
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
