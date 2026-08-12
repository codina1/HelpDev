"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import type { AuthUser, UpdateProfileRequest } from "@/types/auth";

type ProfileEditModalProps = {
  user: AuthUser;
  open: boolean;
  onClose: () => void;
};

const emptyForm: UpdateProfileRequest = {
  firstName: "",
  lastName: "",
  email: "",
  profileImageUrl: "",
  expertise: "",
  interests: "",
};

export function ProfileEditModal({ user, open, onClose }: ProfileEditModalProps) {
  const { saveProfile } = useAuth();
  const [form, setForm] = useState<UpdateProfileRequest>(emptyForm);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    if (!open) return;
    setForm({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      profileImageUrl: user.profileImageUrl,
      expertise: user.expertise,
      interests: user.interests,
    });
    setError(null);
  }, [open, user]);

  if (!open) return null;

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      await saveProfile(form);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "ذخیره ناموفق بود.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className="fixed inset-0 z-[100] flex items-center justify-center p-4">
      <button type="button" className="absolute inset-0 bg-black/75 backdrop-blur-sm" onClick={onClose} aria-label="بستن" />
      <form onSubmit={handleSubmit} className="relative w-full max-w-lg rounded-2xl border border-white/10 bg-[#12182a] p-6 shadow-2xl">
        <h2 className="text-lg font-bold text-white">ویرایش پروفایل</h2>
        <div className="mt-5 grid gap-4 sm:grid-cols-2">
          <Field label="نام" value={form.firstName} onChange={(v) => setForm({ ...form, firstName: v })} />
          <Field label="نام خانوادگی" value={form.lastName} onChange={(v) => setForm({ ...form, lastName: v })} />
        </div>
        <div className="mt-4 space-y-4">
          <Field label="ایمیل" value={form.email} onChange={(v) => setForm({ ...form, email: v })} dir="ltr" />
          <Field label="آدرس تصویر" value={form.profileImageUrl} onChange={(v) => setForm({ ...form, profileImageUrl: v })} dir="ltr" />
          <Field label="تخصص" value={form.expertise} onChange={(v) => setForm({ ...form, expertise: v })} />
          <label className="block">
            <span className="mb-2 block text-[13px] font-semibold text-slate-300">علاقه‌مندی‌ها</span>
            <textarea
              value={form.interests}
              onChange={(e) => setForm({ ...form, interests: e.target.value })}
              className="field-input min-h-24 resize-y"
              placeholder="React, AI, DevOps"
            />
          </label>
        </div>
        {error && <p className="mt-3 text-[13px] text-red-400">{error}</p>}
        <div className="mt-5 flex gap-3">
          <button type="button" onClick={onClose} className="focus-ring flex-1 rounded-xl border border-white/10 py-2.5 text-[13px] font-semibold text-slate-300">
            انصراف
          </button>
          <button type="submit" disabled={loading} className="focus-ring flex-1 rounded-xl bg-gradient-to-l from-violet-600 to-indigo-600 py-2.5 text-[13px] font-bold text-white disabled:opacity-60">
            {loading ? "در حال ذخیره..." : "ذخیره"}
          </button>
        </div>
      </form>
    </div>
  );
}

function Field({
  label,
  value,
  onChange,
  dir,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  dir?: "ltr";
}) {
  return (
    <label className="block">
      <span className="mb-2 block text-[13px] font-semibold text-slate-300">{label}</span>
      <input
        value={value}
        onChange={(e) => onChange(e.target.value)}
        dir={dir}
        className={`field-input ${dir === "ltr" ? "text-left" : ""}`}
        required={label === "نام" || label === "نام خانوادگی"}
      />
    </label>
  );
}
