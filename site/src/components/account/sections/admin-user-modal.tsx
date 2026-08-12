"use client";

import { useEffect, useState } from "react";
import type { AdminUserDetail, UpdateAdminUserRequest } from "@/lib/profile-api";

type AdminUserModalProps = {
  open: boolean;
  mode: "view" | "edit";
  user: AdminUserDetail | null;
  loading?: boolean;
  saving?: boolean;
  error?: string | null;
  onClose: () => void;
  onSwitchToEdit: () => void;
  onSave: (request: UpdateAdminUserRequest) => Promise<void>;
};

const ROLES = ["User", "Writer", "Admin"] as const;

export function AdminUserModal({
  open,
  mode,
  user,
  loading = false,
  saving = false,
  error = null,
  onClose,
  onSwitchToEdit,
  onSave,
}: AdminUserModalProps) {
  const [form, setForm] = useState<UpdateAdminUserRequest>({
    firstName: "",
    lastName: "",
    email: "",
    profileImageUrl: "",
    expertise: "",
    interests: "",
    role: "User",
  });

  useEffect(() => {
    if (!open || !user) return;
    setForm({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      profileImageUrl: user.profileImageUrl,
      expertise: user.expertise,
      interests: user.interests,
      role: user.role,
    });
  }, [open, user]);

  if (!open) return null;

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    await onSave(form);
  }

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <button
        type="button"
        className="absolute inset-0 bg-black/75 backdrop-blur-sm"
        onClick={onClose}
        aria-label="بستن"
      />

      <div className="relative max-h-[90vh] w-full max-w-xl overflow-y-auto rounded-2xl border border-white/10 bg-[#12182a] p-5 shadow-2xl sm:p-6">
        <div className="flex items-start justify-between gap-3">
          <div>
            <h2 className="text-lg font-bold text-white">
              {mode === "edit" ? "ویرایش کاربر" : "جزئیات کاربر"}
            </h2>
            {user && (
              <p dir="ltr" className="mt-1 text-[12px] text-slate-500">
                {user.mobile}
              </p>
            )}
          </div>
          <button
            type="button"
            onClick={onClose}
            className="focus-ring rounded-lg border border-white/10 px-2.5 py-1 text-[12px] text-slate-400 hover:bg-white/[0.04]"
          >
            بستن
          </button>
        </div>

        {loading && (
          <p className="mt-6 text-[13px] text-slate-400">در حال بارگذاری...</p>
        )}

        {!loading && user && mode === "view" && (
          <div className="mt-5 space-y-4">
            <InfoGrid user={user} />
            <button
              type="button"
              onClick={onSwitchToEdit}
              className="focus-ring w-full rounded-xl bg-violet-600 py-2.5 text-[13px] font-bold text-white hover:bg-violet-500"
            >
              ویرایش اطلاعات
            </button>
          </div>
        )}

        {!loading && user && mode === "edit" && (
          <form onSubmit={handleSubmit} className="mt-5 space-y-4">
            <div className="grid gap-3 sm:grid-cols-2">
              <Field
                label="نام"
                value={form.firstName}
                onChange={(value) => setForm({ ...form, firstName: value })}
                required
              />
              <Field
                label="نام خانوادگی"
                value={form.lastName}
                onChange={(value) => setForm({ ...form, lastName: value })}
                required
              />
            </div>
            <Field
              label="ایمیل"
              value={form.email}
              onChange={(value) => setForm({ ...form, email: value })}
              dir="ltr"
            />
            <Field
              label="آدرس تصویر پروفایل"
              value={form.profileImageUrl}
              onChange={(value) => setForm({ ...form, profileImageUrl: value })}
              dir="ltr"
            />
            <Field
              label="تخصص"
              value={form.expertise}
              onChange={(value) => setForm({ ...form, expertise: value })}
            />
            <label className="block">
              <span className="mb-1.5 block text-[12px] font-semibold text-slate-400">
                علاقه‌مندی‌ها
              </span>
              <textarea
                value={form.interests}
                onChange={(e) => setForm({ ...form, interests: e.target.value })}
                className="field-input min-h-20 resize-y"
                placeholder=".NET, AI, Docker"
              />
            </label>
            <label className="block">
              <span className="mb-1.5 block text-[12px] font-semibold text-slate-400">نقش</span>
              <select
                value={form.role}
                onChange={(e) => setForm({ ...form, role: e.target.value })}
                className="field-input"
              >
                {ROLES.map((role) => (
                  <option key={role} value={role}>
                    {role === "Admin" ? "ادمین" : role === "Writer" ? "نویسنده" : "کاربر"}
                  </option>
                ))}
              </select>
            </label>

            {error && <p className="text-[12px] text-red-400">{error}</p>}

            <div className="flex gap-2 pt-1">
              <button
                type="submit"
                disabled={saving}
                className="focus-ring flex-1 rounded-xl bg-violet-600 py-2.5 text-[13px] font-bold text-white hover:bg-violet-500 disabled:opacity-60"
              >
                {saving ? "در حال ذخیره..." : "ذخیره تغییرات"}
              </button>
              <button
                type="button"
                onClick={onClose}
                className="focus-ring rounded-xl border border-white/10 px-4 py-2.5 text-[13px] text-slate-300 hover:bg-white/[0.04]"
              >
                انصراف
              </button>
            </div>
          </form>
        )}

        {error && mode === "view" && (
          <p className="mt-4 text-[12px] text-red-400">{error}</p>
        )}
      </div>
    </div>
  );
}

function InfoGrid({ user }: { user: AdminUserDetail }) {
  const rows = [
    { label: "نام نمایشی", value: user.displayName },
    { label: "نام", value: `${user.firstName} ${user.lastName}`.trim() || "—" },
    { label: "موبایل", value: user.mobile, ltr: true },
    { label: "ایمیل", value: user.email || "—" },
    { label: "نقش", value: user.role },
    { label: "تخصص", value: user.expertise || "—" },
    { label: "علاقه‌مندی‌ها", value: user.interests || "—" },
    { label: "تکمیل پروفایل", value: `${user.profileCompletionPercent}%` },
    {
      label: "عضویت",
      value: user.createdAt ? new Date(user.createdAt).toLocaleDateString("fa-IR") : "—",
    },
    {
      label: "آخرین ورود",
      value: user.lastLogin
        ? new Date(user.lastLogin).toLocaleString("fa-IR")
        : "—",
    },
  ];

  return (
    <dl className="space-y-2.5">
      {rows.map((row) => (
        <div
          key={row.label}
          className="flex items-start justify-between gap-4 rounded-xl border border-white/[0.06] px-3 py-2.5"
        >
          <dt className="shrink-0 text-[12px] text-slate-500">{row.label}</dt>
          <dd
            dir={row.ltr ? "ltr" : undefined}
            className="text-end text-[13px] font-medium text-slate-200"
          >
            {row.value}
          </dd>
        </div>
      ))}
    </dl>
  );
}

function Field({
  label,
  value,
  onChange,
  dir,
  required,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  dir?: "ltr" | "rtl";
  required?: boolean;
}) {
  return (
    <label className="block">
      <span className="mb-1.5 block text-[12px] font-semibold text-slate-400">{label}</span>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        dir={dir}
        required={required}
        className="field-input"
      />
    </label>
  );
}
