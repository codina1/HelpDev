"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { NavIcon } from "@/components/account/dashboard/dashboard-icons";
import { getUserDisplayName, getUserInitials, type AuthUser } from "@/types/auth";

type DashboardTopbarProps = {
  user: AuthUser;
  onMenuClick?: () => void;
};

export function DashboardTopbar({ user, onMenuClick }: DashboardTopbarProps) {
  const router = useRouter();
  const searchRef = useRef<HTMLInputElement>(null);
  const [query, setQuery] = useState("");
  const [menuOpen, setMenuOpen] = useState(false);

  useEffect(() => {
    function onKeyDown(event: KeyboardEvent) {
      if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        searchRef.current?.focus();
      }
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, []);

  function handleSearch(event: React.FormEvent) {
    event.preventDefault();
    const params = new URLSearchParams();
    if (query.trim()) params.set("q", query.trim());
    router.push(`/search?${params.toString()}`);
  }

  const canCreateContent = user.role === "Writer" || user.role === "Admin";

  return (
    <header className="sticky top-0 z-20 border-b border-white/[0.06] bg-[#080a12]/95 backdrop-blur-xl">
      <div className="flex h-[60px] items-center gap-3 px-4 lg:px-5">
        {/* راست — زنگ + ایجاد محتوا (لبه نزدیک سایدبار) */}
        <div className="flex shrink-0 items-center gap-2">
          <button
            type="button"
            onClick={onMenuClick}
            className="focus-ring flex h-9 w-9 items-center justify-center rounded-lg border border-white/10 text-slate-400 lg:hidden"
            aria-label="منو"
          >
            <NavIcon name="menu" size={18} />
          </button>

          <button
            type="button"
            className="focus-ring relative flex h-9 w-9 items-center justify-center rounded-xl border border-white/10 bg-[#111827]/60 text-slate-400 transition-colors hover:bg-white/[0.05] hover:text-slate-200"
            aria-label="اعلان‌ها"
          >
            <NavIcon name="bell" size={17} />
            <span className="absolute end-2 top-1.5 h-1.5 w-1.5 rounded-full bg-violet-500 ring-2 ring-[#080a12]" />
          </button>

          {canCreateContent && (
            <Link
              href="/write"
              className="focus-ring hidden items-center gap-1 rounded-xl bg-violet-600 px-3.5 py-2 text-[12px] font-bold text-white shadow-[0_4px_14px_rgba(124,58,237,0.28)] transition-colors hover:bg-violet-500 sm:inline-flex"
            >
              <span className="text-[14px] leading-none">+</span>
              ایجاد محتوا
            </Link>
          )}
        </div>

        {/* وسط — جستجو */}
        <div className="flex min-w-0 flex-1 justify-center">
          <form
            onSubmit={handleSearch}
            className="relative w-full max-w-[340px] sm:max-w-[380px]"
          >
            <span className="pointer-events-none absolute inset-y-0 start-3 flex items-center text-slate-500">
              <NavIcon name="search" size={14} />
            </span>
            <input
              ref={searchRef}
              type="search"
              value={query}
              onChange={(e) => setQuery(e.target.value)}
              placeholder="جستجو در اخبار، ابزارها، دوره‌ها..."
              className="focus-ring h-9 w-full rounded-xl border border-white/[0.08] bg-[#111827] pe-[4.25rem] ps-8 text-[12px] text-slate-200 outline-none placeholder:text-slate-600"
            />
            <kbd className="pointer-events-none absolute inset-y-0 end-2 my-auto hidden h-5 items-center rounded border border-white/10 bg-white/[0.04] px-1.5 text-[9px] font-medium text-slate-500 md:inline-flex">
              Ctrl + K
            </kbd>
          </form>
        </div>

        {/* چپ — پروفایل (دور از سایدبار، مثل طرح اصلی) */}
        <div className="flex shrink-0 items-center">
          <div className="relative">
            <button
              type="button"
              onClick={() => setMenuOpen((open) => !open)}
              className="focus-ring flex items-center gap-2 rounded-xl border border-white/10 bg-[#111827]/80 py-1.5 pe-2.5 ps-1.5 transition-colors hover:bg-white/[0.05]"
            >
              <UserAvatar user={user} />
              <span className="hidden max-w-[110px] truncate text-[12px] font-semibold text-slate-200 md:inline">
                {getUserDisplayName(user)}
              </span>
              <NavIcon name="chevron" size={14} className="hidden text-slate-500 sm:block" />
            </button>

            {menuOpen && (
              <>
                <button
                  type="button"
                  className="fixed inset-0 z-40"
                  aria-label="بستن منو"
                  onClick={() => setMenuOpen(false)}
                />
                <div className="absolute end-0 top-full z-50 mt-2 w-44 rounded-xl border border-white/10 bg-[#12182a] p-1 shadow-2xl">
                  <Link
                    href="/profile?tab=profile"
                    onClick={() => setMenuOpen(false)}
                    className="block rounded-lg px-3 py-2 text-[13px] text-slate-200 hover:bg-white/[0.06]"
                  >
                    پروفایل من
                  </Link>
                  <Link
                    href="/profile?tab=settings"
                    onClick={() => setMenuOpen(false)}
                    className="block rounded-lg px-3 py-2 text-[13px] text-slate-200 hover:bg-white/[0.06]"
                  >
                    تنظیمات
                  </Link>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </header>
  );
}

function UserAvatar({ user }: { user: AuthUser }) {
  if (user.profileImageUrl) {
    return (
      <img
        src={user.profileImageUrl}
        alt={getUserDisplayName(user)}
        className="h-7 w-7 rounded-full border border-white/10 object-cover"
      />
    );
  }

  return (
    <span className="flex h-7 w-7 items-center justify-center rounded-full bg-violet-600/35 text-[10px] font-bold text-violet-100">
      {getUserInitials(user)}
    </span>
  );
}
