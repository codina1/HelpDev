"use client";

import { Badge } from "@/components/ui/badge";
import { getVisibleAccountMenu, type AccountSection } from "@/lib/account-menu";
import { getUserDisplayName, getUserInitials, type AuthUser } from "@/types/auth";

type AccountSidebarProps = {
  user: AuthUser;
  activeSection: AccountSection;
  onSectionChange: (section: AccountSection) => void;
};

export function AccountSidebar({
  user,
  activeSection,
  onSectionChange,
}: AccountSidebarProps) {
  const menu = getVisibleAccountMenu(user.role);

  return (
    <aside className="ui-panel flex h-fit flex-col p-5 lg:sticky lg:top-[78px]">
      <div className="flex flex-col items-center border-b border-white/10 pb-5 text-center">
        <ProfileAvatar user={user} />
        <p className="mt-4 text-[15px] font-bold text-white">{getUserDisplayName(user)}</p>
        <p dir="ltr" className="mt-1 text-[12px] text-slate-400">{user.mobile}</p>
        <div className="mt-3">
          <RoleBadge role={user.role} />
        </div>
        <p className="mt-4 text-[12px] text-slate-400">
          تکمیل پروفایل: {user.profileCompletionPercent ?? 0}%
        </p>
        <div className="mt-2 h-2 w-full overflow-hidden rounded-full bg-white/10">
          <div
            className="h-full rounded-full bg-gradient-to-l from-violet-500 to-indigo-500 transition-all"
            style={{ width: `${user.profileCompletionPercent ?? 0}%` }}
          />
        </div>
      </div>

      <nav className="mt-5 space-y-1" aria-label="منوی حساب کاربری">
        {menu.map((item) => {
          const active = activeSection === item.id;

          return (
            <button
              key={item.id}
              type="button"
              onClick={() => onSectionChange(item.id)}
              className={[
                "focus-ring flex w-full items-start gap-3 rounded-xl px-3 py-3 text-start transition-colors",
                active
                  ? "bg-violet-500/15 text-white shadow-[inset_0_0_0_1px_rgba(167,139,250,0.25)]"
                  : "text-slate-400 hover:bg-white/[0.04] hover:text-white",
              ].join(" ")}
            >
              <span className="min-w-0">
                <span className="block text-[13px] font-bold">{item.label}</span>
                <span className="mt-0.5 block text-[11px] leading-5 text-slate-500">
                  {item.description}
                </span>
              </span>
            </button>
          );
        })}
      </nav>
    </aside>
  );
}

function ProfileAvatar({ user }: { user: AuthUser }) {
  if (user.profileImageUrl) {
    return (
      <img
        src={user.profileImageUrl}
        alt={getUserDisplayName(user)}
        className="h-24 w-24 rounded-full border border-white/10 object-cover"
      />
    );
  }

  return (
    <div className="flex h-24 w-24 items-center justify-center rounded-full border border-violet-500/30 bg-violet-500/15 text-2xl font-bold text-violet-200">
      {getUserInitials(user)}
    </div>
  );
}

function RoleBadge({ role }: { role: AuthUser["role"] }) {
  if (role === "Admin") {
    return (
      <Badge variant="hot" className="px-2 py-0.5 text-[10px]">
        Admin
      </Badge>
    );
  }

  if (role === "Writer") {
    return (
      <Badge variant="pro" className="px-2 py-0.5 text-[10px]">
        Writer
      </Badge>
    );
  }

  return (
    <Badge variant="updated" className="px-2 py-0.5 text-[10px]">
      User
    </Badge>
  );
}
