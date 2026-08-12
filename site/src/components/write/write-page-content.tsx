"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { AuthModal, useAuth } from "@/components/auth";
import { PageHeader } from "@/components/layout";
import {
  CONTENT_TYPE_OPTIONS,
  createContent,
  slugifyTitle,
  type ContentDetail,
  type ContentStatusOption,
  type ContentTypeOption,
  type CreateContentRequest,
} from "@/lib/content-api";

const emptyForm: CreateContentRequest = {
  title: "",
  slug: "",
  body: "",
  type: "Article",
  status: "Draft",
};

export function WritePageContent() {
  const { user, token, isReady } = useAuth();
  const [authOpen, setAuthOpen] = useState(false);
  const [form, setForm] = useState<CreateContentRequest>(emptyForm);
  const [slugTouched, setSlugTouched] = useState(false);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [created, setCreated] = useState<ContentDetail | null>(null);

  const canWrite = user?.role === "Writer" || user?.role === "Admin";

  useEffect(() => {
    if (slugTouched) return;
    if (!form.title.trim()) {
      setForm((prev) => ({ ...prev, slug: "" }));
      return;
    }
    setForm((prev) => ({ ...prev, slug: slugifyTitle(prev.title) }));
  }, [form.title, slugTouched]);

  if (!isReady) {
    return (
      <div className="ui-panel p-6 text-[13px] text-slate-400">
        در حال بارگذاری...
      </div>
    );
  }

  if (!user || !token) {
    return (
      <>
        <PageHeader
          title="نویسنده شو"
          description="برای انتشار محتوا ابتدا وارد حساب کاربری شوید."
        />
        <div className="ui-panel space-y-4 p-6">
          <p className="ui-body">
            بعد از ورود، اگر نقش شما نویسنده یا ادمین باشد می‌توانید مطلب ثبت کنید.
          </p>
          <button
            type="button"
            onClick={() => setAuthOpen(true)}
            className="focus-ring rounded-xl bg-violet-600 px-5 py-2.5 text-[13px] font-bold text-white hover:bg-violet-500"
          >
            ورود / ثبت‌نام
          </button>
        </div>
        <AuthModal open={authOpen} onClose={() => setAuthOpen(false)} />
      </>
    );
  }

  if (!canWrite) {
    return (
      <>
        <PageHeader
          title="نویسنده شو"
          description="حساب شما هنوز نقش نویسنده ندارد."
        />
        <div className="ui-panel space-y-4 p-6">
          <p className="ui-body">
            نقش فعلی شما <strong className="text-white">{user.role}</strong> است.
            برای انتشار مطلب، یک ادمین باید نقش شما را به Writer ارتقا دهد.
          </p>
          <div className="flex flex-wrap gap-2">
            <Link
              href="/profile"
              className="focus-ring rounded-xl border border-white/10 px-4 py-2.5 text-[13px] font-semibold text-slate-200 hover:bg-white/[0.04]"
            >
              رفتن به پروفایل
            </Link>
            <Link
              href="/"
              className="focus-ring rounded-xl bg-violet-600 px-4 py-2.5 text-[13px] font-bold text-white hover:bg-violet-500"
            >
              صفحه اصلی
            </Link>
          </div>
        </div>
      </>
    );
  }

  if (created) {
    return (
      <>
        <PageHeader
          title="محتوا ثبت شد"
          description="مطلب شما با موفقیت ذخیره شد."
        />
        <div className="ui-panel space-y-4 p-6">
          <div className="rounded-xl border border-emerald-500/25 bg-emerald-500/10 px-4 py-3 text-[13px] text-emerald-200">
            «{created.title}» با وضعیت {created.status === "Published" ? "منتشرشده" : "پیش‌نویس"} ذخیره شد.
          </div>
          <dl className="space-y-2 text-[13px]">
            <div className="flex justify-between gap-4 border-b border-white/[0.06] py-2">
              <dt className="text-slate-500">اسلاگ</dt>
              <dd dir="ltr" className="font-medium text-slate-200">
                {created.slug}
              </dd>
            </div>
            <div className="flex justify-between gap-4 border-b border-white/[0.06] py-2">
              <dt className="text-slate-500">نوع</dt>
              <dd className="font-medium text-slate-200">{created.type}</dd>
            </div>
          </dl>
          <div className="flex flex-wrap gap-2 pt-2">
            <button
              type="button"
              onClick={() => {
                setCreated(null);
                setForm(emptyForm);
                setSlugTouched(false);
                setError(null);
              }}
              className="focus-ring rounded-xl bg-violet-600 px-4 py-2.5 text-[13px] font-bold text-white hover:bg-violet-500"
            >
              نوشتن مطلب جدید
            </button>
            <Link
              href="/profile?tab=content"
              className="focus-ring rounded-xl border border-white/10 px-4 py-2.5 text-[13px] font-semibold text-slate-200 hover:bg-white/[0.04]"
            >
              محتوای من
            </Link>
          </div>
        </div>
      </>
    );
  }

  async function handleSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!token) return;

    setSaving(true);
    setError(null);

    try {
      const payload: CreateContentRequest = {
        ...form,
        title: form.title.trim(),
        slug: form.slug.trim().toLowerCase(),
        body: form.body.trim(),
      };
      const result = await createContent(token, payload);
      setCreated(result);
    } catch (err) {
      setError(err instanceof Error ? err.message : "ثبت محتوا ناموفق بود.");
    } finally {
      setSaving(false);
    }
  }

  return (
    <>
      <PageHeader
        title="نویسنده شو"
        description="عنوان، اسلاگ و متن مطلب را وارد کنید و به‌صورت پیش‌نویس یا منتشرشده ذخیره کنید."
      />

      <form onSubmit={handleSubmit} className="ui-panel space-y-5 p-5 sm:p-6">
        <div className="rounded-xl border border-violet-500/20 bg-violet-500/10 px-4 py-3 text-[12px] text-violet-100">
          وارد شده به‌عنوان{" "}
          <strong>{user.displayName || user.mobile}</strong> — نقش: {user.role}
        </div>

        <label className="block">
          <span className="mb-1.5 block text-[13px] font-semibold text-slate-300">عنوان</span>
          <input
            value={form.title}
            onChange={(e) => setForm({ ...form, title: e.target.value })}
            className="field-input"
            placeholder="مثلاً راهنمای شروع با ASP.NET Core"
            required
            maxLength={300}
          />
        </label>

        <label className="block">
          <span className="mb-1.5 block text-[13px] font-semibold text-slate-300">
            اسلاگ (انگلیسی)
          </span>
          <input
            dir="ltr"
            value={form.slug}
            onChange={(e) => {
              setSlugTouched(true);
              setForm({ ...form, slug: e.target.value });
            }}
            className="field-input"
            placeholder="aspnet-core-getting-started"
            required
            pattern="[a-z0-9]+(?:-[a-z0-9]+)*"
            title="فقط حروف انگلیسی کوچک، عدد و خط تیره"
          />
          <span className="mt-1.5 block text-[11px] text-slate-500">
            فقط a-z، عدد و - — حداقل ۲ کاراکتر
          </span>
        </label>

        <div className="grid gap-4 sm:grid-cols-2">
          <label className="block">
            <span className="mb-1.5 block text-[13px] font-semibold text-slate-300">نوع محتوا</span>
            <select
              value={form.type}
              onChange={(e) =>
                setForm({ ...form, type: e.target.value as ContentTypeOption })
              }
              className="field-input"
            >
              {CONTENT_TYPE_OPTIONS.map((option) => (
                <option key={option.value} value={option.value}>
                  {option.label}
                </option>
              ))}
            </select>
          </label>

          <label className="block">
            <span className="mb-1.5 block text-[13px] font-semibold text-slate-300">وضعیت</span>
            <select
              value={form.status}
              onChange={(e) =>
                setForm({
                  ...form,
                  status: e.target.value as ContentStatusOption,
                })
              }
              className="field-input"
            >
              <option value="Draft">پیش‌نویس</option>
              <option value="Published">منتشرشده</option>
            </select>
          </label>
        </div>

        <label className="block">
          <span className="mb-1.5 block text-[13px] font-semibold text-slate-300">متن محتوا</span>
          <textarea
            value={form.body}
            onChange={(e) => setForm({ ...form, body: e.target.value })}
            className="field-input min-h-[240px] resize-y leading-7"
            placeholder="متن کامل مطلب را اینجا بنویسید..."
            required
          />
        </label>

        {error && (
          <p className="rounded-xl border border-red-500/25 bg-red-500/10 px-4 py-3 text-[13px] text-red-300">
            {error}
          </p>
        )}

        <div className="flex flex-wrap gap-2">
          <button
            type="submit"
            disabled={saving}
            className="focus-ring rounded-xl bg-violet-600 px-5 py-2.5 text-[13px] font-bold text-white hover:bg-violet-500 disabled:opacity-60"
          >
            {saving
              ? "در حال ذخیره..."
              : form.status === "Published"
                ? "انتشار محتوا"
                : "ذخیره پیش‌نویس"}
          </button>
          <button
            type="button"
            onClick={() => {
              setForm(emptyForm);
              setSlugTouched(false);
              setError(null);
            }}
            className="focus-ring rounded-xl border border-white/10 px-4 py-2.5 text-[13px] font-semibold text-slate-300 hover:bg-white/[0.04]"
          >
            پاک کردن فرم
          </button>
        </div>
      </form>
    </>
  );
}
