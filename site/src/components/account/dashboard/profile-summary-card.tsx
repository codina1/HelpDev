"use client";

import { useState } from "react";
import { NavIcon } from "@/components/account/dashboard/dashboard-icons";
import { ProfileEditModal } from "@/components/account/dashboard/profile-edit-modal";
import { INTEREST_TAG_COLORS, MOCK_STATS } from "@/data/account-dashboard";
import { getUserDisplayName, getUserInitials, type AuthUser } from "@/types/auth";

type ProfileSummaryCardProps = {
  user: AuthUser;
};

export function ProfileSummaryCard({ user }: ProfileSummaryCardProps) {
  const [editOpen, setEditOpen] = useState(false);
  const displayName = getUserDisplayName(user);
  const interestTags = parseInterests(user.interests, user.expertise);

  return (
    <>
      <section className="dash-card p-5 sm:p-6">
        <div className="grid grid-cols-1 items-center gap-6 lg:grid-cols-[auto_minmax(0,1fr)_auto] lg:gap-8">
          {/* آواتار — سمت راست در RTL */}
          <div className="relative mx-auto shrink-0 lg:mx-0">
            {user.profileImageUrl ? (
              <img
                src={user.profileImageUrl}
                alt={displayName}
                className="h-[92px] w-[92px] rounded-full border-[3px] border-[#1e2438] object-cover shadow-[0_0_32px_rgba(139,92,246,0.15)]"
              />
            ) : (
              <div className="flex h-[92px] w-[92px] items-center justify-center rounded-full border-[3px] border-[#1e2438] bg-gradient-to-br from-violet-600 to-indigo-700 text-2xl font-black text-white shadow-[0_0_32px_rgba(139,92,246,0.2)]">
                {getUserInitials(user)}
              </div>
            )}
            <button
              type="button"
              onClick={() => setEditOpen(true)}
              className="focus-ring absolute bottom-0 end-0 flex h-7 w-7 items-center justify-center rounded-full bg-violet-600 text-white shadow-[0_2px_8px_rgba(124,58,237,0.5)]"
              aria-label="ویرایش پروفایل"
            >
              <NavIcon name="pencil" size={13} className="text-white" />
            </button>
          </div>

          {/* اطلاعات کاربر + علاقه‌مندی‌ها — وسط */}
          <div className="min-w-0 space-y-3 text-center lg:text-start">
            <div>
              <div className="flex flex-wrap items-center justify-center gap-2 lg:justify-start">
                <h1 className="text-xl font-extrabold text-white sm:text-[22px]">
                  {displayName}
                </h1>
                <RoleBadge role={user.role} />
              </div>
              <p dir="ltr" className="mt-1.5 inline-block text-[13px] text-slate-400">
                {formatMobile(user.mobile)}
              </p>
              <p className="mt-1 text-[12px] text-slate-500">عضو از خرداد ۱۴۰۳</p>
            </div>

            <div>
              <p className="mb-2 text-[11px] font-semibold text-slate-500">
                علاقه‌مندی‌های اصلی
              </p>
              <div className="flex flex-wrap justify-center gap-1.5 lg:justify-start">
                {interestTags.map((tag, index) => (
                  <span
                    key={tag}
                    className={`rounded-md border px-2.5 py-0.5 text-[11px] font-semibold ${INTEREST_TAG_COLORS[index % INTEREST_TAG_COLORS.length]}`}
                  >
                    {tag}
                  </span>
                ))}
              </div>
            </div>
          </div>

          {/* آمار — سمت چپ در RTL */}
          <div className="flex items-center justify-center gap-5 border-t border-white/[0.06] pt-5 sm:gap-6 lg:border-t-0 lg:border-s lg:pt-0 lg:ps-6">
            <StatBox label="مقالات خوانده‌شده" value={MOCK_STATS.readArticles} />
            <StatBox label="مسیر دنبال‌شده" value={MOCK_STATS.followedPaths} />
            <StatBox label="یادداشت ذخیره‌شده" value={MOCK_STATS.savedNotes} />
          </div>
        </div>
      </section>

      <ProfileEditModal user={user} open={editOpen} onClose={() => setEditOpen(false)} />
    </>
  );
}

function StatBox({ label, value }: { label: string; value: number }) {
  return (
    <div className="min-w-[68px] text-center">
      <p className="text-[26px] font-black leading-none text-white">{value}</p>
      <p className="mt-1.5 text-[10px] leading-4 text-slate-500">{label}</p>
    </div>
  );
}

function RoleBadge({ role }: { role: AuthUser["role"] }) {
  const config = {
    Admin: {
      label: "ادمین",
      className: "bg-amber-400/15 text-amber-300 border-amber-400/30",
    },
    Writer: {
      label: "نویسنده",
      className: "bg-cyan-500/15 text-cyan-300 border-cyan-500/30",
    },
    User: {
      label: "کاربر",
      className: "bg-emerald-500/15 text-emerald-300 border-emerald-500/30",
    },
  }[role];

  return (
    <span
      className={`rounded-md border px-2 py-0.5 text-[10px] font-bold ${config.className}`}
    >
      {config.label}
    </span>
  );
}

function formatMobile(mobile: string) {
  const digits = mobile.replace(/\D/g, "");
  if (digits.length === 11) {
    return `${digits.slice(0, 4)} ${digits.slice(4, 7)} ${digits.slice(7)}`;
  }
  return mobile;
}

function parseInterests(interests: string, expertise: string) {
  const fromInterests = interests
    .split(/[,،\n]/)
    .map((item) => item.trim())
    .filter(Boolean);

  if (fromInterests.length > 0) return fromInterests.slice(0, 5);

  if (expertise) return [expertise];

  return [".NET", "C#", "ASP.NET Core", "AI", "Docker"];
}
