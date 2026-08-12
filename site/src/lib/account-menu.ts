import type { UserRole } from "@/types/auth";

export type AccountSection =
  | "profile"
  | "admin"
  | "content"
  | "favorites"
  | "saved"
  | "settings";

export type AccountMenuItem = {
  id: AccountSection;
  label: string;
  description: string;
  roles?: UserRole[];
};

export const ACCOUNT_MENU: AccountMenuItem[] = [
  {
    id: "profile",
    label: "پروفایل من",
    description: "داشبورد و اطلاعات شخصی",
  },
  {
    id: "admin",
    label: "پنل ادمین",
    description: "مدیریت کاربران و محتوا",
    roles: ["Admin"],
  },
  {
    id: "content",
    label: "محتوای من",
    description: "مقالات و مطالب منتشرشده",
    roles: ["Writer", "Admin"],
  },
  {
    id: "favorites",
    label: "علاقه‌مندی‌ها",
    description: "آیتم‌های مورد علاقه",
  },
  {
    id: "saved",
    label: "ذخیره‌شده‌ها",
    description: "مطالب ذخیره‌شده",
  },
  {
    id: "settings",
    label: "تنظیمات",
    description: "اعلان‌ها و حریم خصوصی",
  },
];

export function getVisibleAccountMenu(role: UserRole): AccountMenuItem[] {
  return ACCOUNT_MENU.filter(
    (item) => !item.roles || item.roles.includes(role),
  );
}

export function isAccountSection(value: string | null): value is AccountSection {
  return ACCOUNT_MENU.some((item) => item.id === value);
}

export function getDefaultSection(role: UserRole): AccountSection {
  return "profile";
}
