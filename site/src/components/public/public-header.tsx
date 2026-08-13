"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import { AuthModal, useAuth } from "@/components/auth";
import { GlobalSearchPalette } from "@/components/search/global-search-palette";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PUBLIC_PRODUCTS_NAV } from "@/lib/public/nav-v2";
import { SITE } from "@/lib/constants";
import { getUserDisplayName, getUserInitials } from "@/types/auth";

/**
 * Minimal glass public header — Linear/Vercel density, RTL.
 * Logo at inline-start (right), nav centered, compact actions at inline-end.
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
    return pathname === href || pathname.startsWith(`${href}/`);
  }

  return (
    <>
      <header className="pub-navbar sticky top-0 z-50 border-b border-white/[0.08] bg-[rgba(8,10,18,0.68)] backdrop-blur-xl backdrop-saturate-150">
        <PublicContainer size="wide" className="relative flex h-[72px] items-center">
          <Link href="/" className="focus-ring relative z-10 flex shrink-0 items-center gap-2 rounded-md">
            <span
              className="flex h-6 w-6 items-center justify-center rounded-md bg-white/[0.08] text-[10px] font-bold text-white"
              aria-hidden
            >
              H
            </span>
            <span className="text-[14px] font-semibold tracking-tight text-white">{SITE.name}</span>
          </Link>

          <nav
            className="pointer-events-none absolute inset-x-0 hidden items-center justify-center md:flex"
            aria-label="ناوبری اصلی"
          >
            <div className="pointer-events-auto flex items-center gap-0.5">
              {PUBLIC_PRODUCTS_NAV.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className={[
                    "focus-ring rounded-md px-3 py-1.5 text-[13px] font-medium transition-colors",
                    isActive(item.href)
                      ? "text-white"
                      : "text-white/55 hover:text-white",
                  ].join(" ")}
                  aria-current={isActive(item.href) ? "page" : undefined}
                >
                  {item.label}
                </Link>
              ))}
            </div>
          </nav>

          <div className="relative z-10 ms-auto flex shrink-0 items-center gap-1">
            <button
              type="button"
              onClick={() => setPaletteOpen(true)}
              className="focus-ring inline-flex h-8 w-8 items-center justify-center rounded-md text-white/55 transition-colors hover:bg-white/[0.06] hover:text-white"
              aria-label="جستجوی AI — Ctrl+K"
            >
              <SearchIcon />
            </button>

            {isReady && user ? (
              <>
                <Link
                  href="/dashboard"
                  className="focus-ring hidden h-8 items-center rounded-md px-2.5 text-[12px] font-medium text-white/70 transition-colors hover:bg-white/[0.06] hover:text-white sm:inline-flex"
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
                  className="focus-ring hidden h-8 items-center rounded-md px-2 text-[12px] font-medium text-white/45 transition-colors hover:text-white/80 lg:inline-flex"
                >
                  خروج
                </button>
              </>
            ) : (
              <button
                type="button"
                onClick={() => setAuthOpen(true)}
                className="focus-ring inline-flex h-8 items-center rounded-md border border-white/[0.12] px-3 text-[12px] font-medium text-white/90 transition-colors hover:border-white/20 hover:bg-white/[0.06]"
              >
                ورود
              </button>
            )}

            <button
              type="button"
              className="focus-ring inline-flex h-8 w-8 items-center justify-center rounded-md text-white/55 hover:bg-white/[0.06] hover:text-white md:hidden"
              aria-label={menuOpen ? "بستن منو" : "باز کردن منو"}
              aria-expanded={menuOpen}
              onClick={() => setMenuOpen((open) => !open)}
            >
              <MenuIcon open={menuOpen} />
            </button>
          </div>
        </PublicContainer>

        {menuOpen ? (
          <div className="border-t border-white/[0.08] bg-[rgba(8,10,18,0.92)] backdrop-blur-xl md:hidden">
            <nav className="mx-auto flex max-w-[1400px] flex-col px-4 py-2" aria-label="ناوبری موبایل">
              {PUBLIC_PRODUCTS_NAV.map((item) => (
                <Link
                  key={item.href}
                  href={item.href}
                  className={[
                    "focus-ring rounded-md px-2 py-2.5 text-[13px] font-medium",
                    isActive(item.href) ? "text-white" : "text-white/60",
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

function UserAvatar({ user }: { user: NonNullable<ReturnType<typeof useAuth>["user"]> }) {
  if (user.profileImageUrl) {
    return (
      // eslint-disable-next-line @next/next/no-img-element
      <img
        src={user.profileImageUrl}
        alt=""
        className="h-7 w-7 rounded-full object-cover"
      />
    );
  }
  return (
    <span className="flex h-7 w-7 items-center justify-center rounded-full bg-white/[0.08] text-[10px] font-semibold text-white/80">
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

function MenuIcon({ open }: { open: boolean }) {
  return (
    <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.75" aria-hidden>
      {open ? (
        <path d="M6 6l12 12M18 6L6 18" />
      ) : (
        <path d="M4 8h16M4 16h16" />
      )}
    </svg>
  );
}
