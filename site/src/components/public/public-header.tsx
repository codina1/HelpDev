"use client";

import Link from "next/link";
import { usePathname } from "next/navigation";
import { useEffect, useState } from "react";
import { AuthModal, useAuth } from "@/components/auth";
import { NotificationCenter } from "@/components/notifications/notification-center";
import { GlobalSearchPalette } from "@/components/search/global-search-palette";
import { GlowButton } from "@/components/ui/public/v2/glow-button";
import { PremiumBadge } from "@/components/ui/public/v2/premium-badge";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { PUBLIC_PRODUCTS_NAV } from "@/lib/public/nav-v2";
import { SITE } from "@/lib/constants";
import { getUserDisplayName, getUserInitials } from "@/types/auth";

/**
 * Sprint 50B premium public header — glass, sticky, compact products nav.
 */
export function PublicHeader() {
  const pathname = usePathname();
  const { user, logout, isReady } = useAuth();
  const [authOpen, setAuthOpen] = useState(false);
  const [paletteOpen, setPaletteOpen] = useState(false);
  const [productsOpen, setProductsOpen] = useState(false);

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
    setProductsOpen(false);
  }, [pathname]);

  function isActive(href: string) {
    return pathname === href || pathname.startsWith(`${href}/`);
  }

  return (
    <>
      <header className="sticky top-0 z-50 border-b border-[color:var(--pub-glass-border)] bg-[color:color-mix(in_srgb,var(--pub-bg)_72%,transparent)] backdrop-blur-xl backdrop-saturate-150">
        <PublicContainer size="wide" className="flex h-14 items-center gap-3 lg:h-[58px] lg:gap-4">
          <Link href="/" className="focus-ring group flex shrink-0 items-center gap-2 rounded-xl">
            <span
              className="flex h-8 w-8 items-center justify-center rounded-xl bg-gradient-to-br from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] text-sm text-white shadow-[0_0_20px_var(--pub-glow)]"
              aria-hidden
            >
              ⚡
            </span>
            <span className="hidden text-[14px] font-extrabold tracking-tight text-[color:var(--pub-fg)] sm:block">
              {SITE.name}
            </span>
          </Link>

          <div className="relative hidden md:block">
            <button
              type="button"
              className="focus-ring inline-flex items-center gap-1.5 rounded-lg px-2.5 py-1.5 text-[13px] font-semibold text-[color:var(--pub-muted)] hover:bg-white/[0.04] hover:text-[color:var(--pub-fg)]"
              aria-expanded={productsOpen}
              aria-haspopup="menu"
              onClick={() => setProductsOpen((v) => !v)}
            >
              محصولات
              <ChevronIcon open={productsOpen} />
            </button>
            {productsOpen ? (
              <div
                role="menu"
                className="pub-glass-strong absolute start-0 top-full z-50 mt-2 min-w-[200px] rounded-xl p-1.5"
              >
                {PUBLIC_PRODUCTS_NAV.map((item) => (
                  <Link
                    key={item.href}
                    role="menuitem"
                    href={item.href}
                    className={[
                      "focus-ring flex rounded-lg px-3 py-2 text-[13px] font-semibold",
                      isActive(item.href)
                        ? "bg-[color:color-mix(in_srgb,var(--pub-primary)_16%,transparent)] text-[color:var(--pub-ai-from)]"
                        : "text-[color:var(--pub-muted)] hover:bg-white/[0.04] hover:text-[color:var(--pub-fg)]",
                    ].join(" ")}
                  >
                    {item.label}
                  </Link>
                ))}
              </div>
            ) : null}
          </div>

          <nav className="scrollbar-thin hidden min-w-0 flex-1 items-center gap-0.5 overflow-x-auto lg:flex" aria-label="محصولات">
            {PUBLIC_PRODUCTS_NAV.map((item) => (
              <Link
                key={item.href}
                href={item.href}
                className={[
                  "focus-ring whitespace-nowrap rounded-lg px-2.5 py-1.5 text-[12px] font-semibold transition xl:text-[13px]",
                  isActive(item.href)
                    ? "bg-[color:color-mix(in_srgb,var(--pub-primary)_16%,transparent)] text-[color:var(--pub-ai-from)]"
                    : "text-[color:var(--pub-muted)] hover:bg-white/[0.04] hover:text-[color:var(--pub-fg)]",
                ].join(" ")}
                aria-current={isActive(item.href) ? "page" : undefined}
              >
                {item.label}
              </Link>
            ))}
          </nav>

          <button
            type="button"
            onClick={() => setPaletteOpen(true)}
            className="focus-ring ms-auto hidden h-9 min-w-[200px] items-center gap-2 rounded-xl border border-[color:var(--pub-glass-border)] bg-white/[0.03] px-3 text-start text-[12px] text-[color:var(--pub-muted)] hover:border-[color:color-mix(in_srgb,var(--pub-primary)_40%,transparent)] md:inline-flex lg:min-w-[240px]"
            aria-label="جستجوی AI — Ctrl+K"
          >
            <SearchIcon />
            <span className="flex-1">جستجوی دانش...</span>
            <kbd className="rounded border border-white/10 px-1.5 py-0.5 font-mono text-[10px]">⌘K</kbd>
          </button>

          <div className="flex shrink-0 items-center gap-1.5 md:ms-0 ms-auto">
            <button
              type="button"
              className="focus-ring inline-flex h-9 w-9 items-center justify-center rounded-xl border border-[color:var(--pub-glass-border)] text-[color:var(--pub-muted)] md:hidden"
              aria-label="جستجو"
              onClick={() => setPaletteOpen(true)}
            >
              <SearchIcon />
            </button>

            {isReady && user ? (
              <>
                <NotificationCenter />
                <GlowButton href="/dashboard" variant="ghost" className="hidden !px-3 !py-1.5 sm:inline-flex">
                  داشبورد
                </GlowButton>
                <Link
                  href="/profile"
                  className="focus-ring flex items-center gap-2 rounded-xl border border-[color:var(--pub-glass-border)] bg-white/[0.03] px-2 py-1.5 hover:border-[color:color-mix(in_srgb,var(--pub-primary)_40%,transparent)]"
                >
                  <UserAvatar user={user} />
                  <span className="hidden max-w-[120px] truncate text-[12px] font-semibold lg:inline">
                    {getUserDisplayName(user)}
                  </span>
                  {user.role === "Admin" || user.role === "Writer" ? (
                    <PremiumBadge variant="primary" className="hidden md:inline-flex">
                      {user.role}
                    </PremiumBadge>
                  ) : null}
                </Link>
                <button
                  type="button"
                  onClick={logout}
                  className="focus-ring hidden rounded-xl px-2.5 py-1.5 text-[12px] font-semibold text-[color:var(--pub-muted)] hover:text-red-300 lg:inline-flex"
                >
                  خروج
                </button>
              </>
            ) : (
              <GlowButton onClick={() => setAuthOpen(true)} className="!px-3.5 !py-2 text-[12px]">
                ورود
              </GlowButton>
            )}
          </div>
        </PublicContainer>
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
        alt={getUserDisplayName(user)}
        className="h-7 w-7 rounded-full border border-[color:var(--pub-glass-border)] object-cover"
      />
    );
  }
  return (
    <span className="flex h-7 w-7 items-center justify-center rounded-full bg-[color:color-mix(in_srgb,var(--pub-primary)_20%,transparent)] text-[10px] font-bold text-[color:var(--pub-ai-from)]">
      {getUserInitials(user)}
    </span>
  );
}

function SearchIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" aria-hidden>
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3-3" />
    </svg>
  );
}

function ChevronIcon({ open }: { open: boolean }) {
  return (
    <svg
      width="12"
      height="12"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      className={open ? "rotate-180 transition" : "transition"}
      aria-hidden
    >
      <path d="m6 9 6 6 6-6" />
    </svg>
  );
}
