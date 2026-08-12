"use client";

import { useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import type { AuthUser, UpdateProfileRequest } from "@/types/auth";

const emptyForm: UpdateProfileRequest = {
  firstName: "",
  lastName: "",
  email: "",
  profileImageUrl: "",
  expertise: "",
  interests: "",
};

type ProfileSectionProps = {
  user: AuthUser;
};

export function ProfileSection({ user }: ProfileSectionProps) {
  const { saveProfile } = useAuth();
  const [form, setForm] = useState<UpdateProfileRequest>(emptyForm);
  const [loading, setLoading] = useState(false);
  const [message, setMessage] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    setForm({
      firstName: user.firstName,
      lastName: user.lastName,
      email: user.email,
      profileImageUrl: user.profileImageUrl,
      expertise: user.expertise,
      interests: user.interests,
    });
  }, [user]);

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    setLoading(true);
    setError(null);
    setMessage(null);

    try {
      await saveProfile(form);
      setMessage("پروفایل با موفقیت ذخیره شد.");
    } catch (err) {
      setError(err instanceof Error ? err.message : "ذخیره پروفایل ناموفق بود.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="space-y-5">
      <div className="grid gap-4 sm:grid-cols-2">
        <Field label="نام">
          <input
            value={form.firstName}
            onChange={(e) => setForm({ ...form, firstName: e.target.value })}
            className="field-input"
            required
          />
        </Field>
        <Field label="نام خانوادگی">
          <input
            value={form.lastName}
            onChange={(e) => setForm({ ...form, lastName: e.target.value })}
            className="field-input"
            required
          />
        </Field>
      </div>

      <Field label="ایمیل">
        <input
          type="email"
          dir="ltr"
          value={form.email}
          onChange={(e) => setForm({ ...form, email: e.target.value })}
          className="field-input text-left"
          placeholder="name@example.com"
        />
      </Field>

      <Field label="آدرس تصویر پروفایل">
        <input
          dir="ltr"
          value={form.profileImageUrl}
          onChange={(e) => setForm({ ...form, profileImageUrl: e.target.value })}
          className="field-input text-left"
          placeholder="https://..."
        />
      </Field>

      <Field label="تخصص">
        <input
          value={form.expertise}
          onChange={(e) => setForm({ ...form, expertise: e.target.value })}
          className="field-input"
          placeholder="مثلاً برنامه‌نویس فول‌استک"
        />
      </Field>

      <Field label="علاقه‌مندی‌ها">
        <textarea
          value={form.interests}
          onChange={(e) => setForm({ ...form, interests: e.target.value })}
          className="field-input min-h-28 resize-y"
          placeholder="React, DevOps, AI, ..."
        />
      </Field>

      {message && <p className="text-[13px] text-emerald-400">{message}</p>}
      {error && <p className="text-[13px] text-red-400">{error}</p>}

      <button
        type="submit"
        disabled={loading}
        className="focus-ring rounded-xl bg-gradient-to-l from-violet-600 to-indigo-600 px-5 py-3 text-[14px] font-bold text-white disabled:opacity-60"
      >
        {loading ? "در حال ذخیره..." : "ذخیره پروفایل"}
      </button>
    </form>
  );
}

function Field({ label, children }: { label: string; children: React.ReactNode }) {
  return (
    <label className="block">
      <span className="mb-2 block text-[13px] font-semibold text-slate-300">{label}</span>
      {children}
    </label>
  );
}
