"use client";

import { useEffect, useMemo } from "react";
import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useAuth } from "@/components/auth";
import { AccountDashboardShell } from "@/components/account/dashboard/account-dashboard-shell";
import { DashboardHome } from "@/components/account/dashboard/dashboard-home";
import { ContentPreferencesCard } from "@/components/account/dashboard/content-preferences-card";
import { AdminSection } from "@/components/account/sections/admin-section";
import { ProfileSection } from "@/components/account/sections/profile-section";
import { PlaceholderSection } from "@/components/account/sections/placeholder-section";
import {
  getVisibleAccountMenu,
  isAccountSection,
  type AccountSection,
} from "@/lib/account-menu";

type AccountPageContentProps = {
  forcedSection?: AccountSection;
};

export function AccountPageContent({ forcedSection }: AccountPageContentProps = {}) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const { user, isReady } = useAuth();
  const tabParam = forcedSection ?? searchParams.get("tab");

  const activeSection = useMemo<AccountSection>(() => {
    if (!user) return "profile";
    const menu = getVisibleAccountMenu(user.role);
    const requested = isAccountSection(tabParam) ? tabParam : "profile";
    return menu.some((item) => item.id === requested) ? requested : "profile";
  }, [user, tabParam]);

  useEffect(() => {
    if (!isReady) return;
    if (!user) return;
    // The admin area moved to the dedicated /admin shell.
    if (activeSection === "admin") {
      router.replace("/admin");
      return;
    }
    if (tabParam && isAccountSection(tabParam)) {
      const menu = getVisibleAccountMenu(user.role);
      if (!menu.some((item) => item.id === tabParam)) {
        router.replace("/profile?tab=profile");
      }
    }
  }, [isReady, user, tabParam, router, activeSection]);

  if (!isReady) {
    return (
      <div className="flex min-h-dvh items-center justify-center bg-[#080a12] text-[13px] text-slate-400">
        در حال بارگذاری پنل کاربری...
      </div>
    );
  }

  if (!user) {
    return (
      <div className="flex min-h-dvh flex-col items-center justify-center gap-4 bg-[#080a12] px-6 text-center">
        <p className="text-[15px] font-semibold text-white">برای مشاهده پنل کاربری وارد شوید</p>
        <p className="max-w-sm text-[13px] leading-6 text-slate-400">
          ابتدا از صفحه اصلی با شماره موبایل خود وارد حساب شوید.
        </p>
        <Link
          href="/"
          className="focus-ring rounded-xl bg-violet-600 px-5 py-2.5 text-[13px] font-bold text-white hover:bg-violet-500"
        >
          بازگشت به صفحه اصلی
        </Link>
      </div>
    );
  }

  function handleSectionChange(section: AccountSection) {
    if (section === "admin") {
      router.push("/admin");
      return;
    }
    router.push(`/profile?tab=${section}`);
  }

  return (
    <AccountDashboardShell
      user={user}
      activeSection={activeSection}
      onSectionChange={handleSectionChange}
    >
      {activeSection === "profile" && <DashboardHome user={user} />}
      {activeSection === "admin" && user.role === "Admin" && <AdminSection />}
      {activeSection === "settings" && (
        <div className="grid gap-5 lg:grid-cols-2">
          <div className="dash-card p-5">
            <h2 className="mb-5 text-[15px] font-bold text-white">ویرایش اطلاعات</h2>
            <ProfileSection user={user} />
          </div>
          <ContentPreferencesCard />
        </div>
      )}
      {activeSection === "content" && (
        <PlaceholderSection
          title="محتوای من"
          description="به‌زودی می‌توانید مطالب خود را اینجا مدیریت کنید."
          actionHref="/write"
          actionLabel="رفتن به بخش نویسنده"
        />
      )}
      {activeSection === "favorites" && (
        <PlaceholderSection
          title="علاقه‌مندی‌ها"
          description="لیست علاقه‌مندی‌های شما اینجا نمایش داده می‌شود."
          actionHref="/favorites"
          actionLabel="مشاهده علاقه‌مندی‌ها"
        />
      )}
      {activeSection === "saved" && (
        <PlaceholderSection
          title="ذخیره‌شده‌ها"
          description="مطالب ذخیره‌شده برای مطالعه بعدی."
          actionHref="/saved"
          actionLabel="مشاهده ذخیره‌شده‌ها"
        />
      )}
    </AccountDashboardShell>
  );
}
