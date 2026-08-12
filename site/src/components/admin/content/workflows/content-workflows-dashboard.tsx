"use client";

import Link from "next/link";
import { useCallback, useEffect, useState } from "react";
import { useAuth } from "@/components/auth";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import {
  createAiContentWorkflow,
  listAiContentWorkflows,
  type AiContentWorkflowListItemDto,
} from "@/lib/api/content-workflows";
import { adminContentWorkflowRoute, ADMIN_ROUTES } from "@/lib/admin/routes";
import { formatDateTimeFa } from "@/lib/admin/content/content-mappers";

export function ContentWorkflowsDashboard() {
  const { token } = useAuth();
  const [items, setItems] = useState<AiContentWorkflowListItemDto[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<unknown>(null);
  const [creating, setCreating] = useState(false);
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");

  const load = useCallback(() => {
    if (!token) {
      setError(new Error("برای مشاهده گردش‌کار باید وارد شوید."));
      setLoading(false);
      return;
    }

    const controller = new AbortController();
    setLoading(true);
    setError(null);
    listAiContentWorkflows(token, controller.signal)
      .then((rows) => {
        setItems(rows ?? []);
        setLoading(false);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(err);
        setLoading(false);
      });

    return () => controller.abort();
  }, [token]);

  useEffect(() => {
    return load();
  }, [load]);

  async function onCreate() {
    if (!token || !title.trim()) return;
    setCreating(true);
    setError(null);
    try {
      const session = await createAiContentWorkflow(token, {
        title: title.trim(),
        description: description.trim() || undefined,
        targetType: "Article",
      });
      window.location.href = adminContentWorkflowRoute(session.id);
    } catch (err) {
      setError(err);
      setCreating(false);
    }
  }

  const active = items.filter((i) => !["Completed", "Cancelled"].includes(i.ideaStatus));
  const drafts = items.filter((i) => i.ideaStatus === "Draft");
  const review = items.filter((i) => i.ideaStatus === "Review" || i.currentStep === "Review");

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title="گردش کار تولید محتوا با AI"
        description="AI پیشنهاد می‌دهد؛ انسان تأیید و منتشر می‌کند. انتشار خودکار وجود ندارد."
      />

      <AdminPageSection title="ایده جدید">
        <div className="space-y-3">
          <input
            className="w-full rounded-md border border-[var(--adm-border)] bg-[var(--adm-surface)] px-3 py-2 text-[13px]"
            placeholder="عنوان ایده"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
          />
          <textarea
            className="min-h-[80px] w-full rounded-md border border-[var(--adm-border)] bg-[var(--adm-surface)] px-3 py-2 text-[13px]"
            placeholder="توضیح کوتاه (اختیاری)"
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          />
          <button
            type="button"
            disabled={creating || !title.trim()}
            onClick={onCreate}
            className="rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50"
          >
            {creating ? "در حال ایجاد…" : "ایجاد با AI"}
          </button>
        </div>
      </AdminPageSection>

      <div className="grid gap-3 sm:grid-cols-3">
        <Stat label="فعال" value={active.length} />
        <Stat label="ایده‌های پیش‌نویس" value={drafts.length} />
        <Stat label="در انتظار بازبینی" value={review.length} />
      </div>

      {loading ? <AdminLoadingState cards={2} rows={4} /> : null}
      {error ? (
        <AdminErrorState error={error} title="بارگذاری گردش‌کار ناموفق بود" onRetry={load} />
      ) : null}

      {!loading && !error ? (
        <AdminPageSection title="گردش‌کارها">
          {items.length === 0 ? (
            <p className="adm-subtle text-[13px]">هنوز گردش‌کاری ثبت نشده است.</p>
          ) : (
            <ul className="space-y-2">
              {items.map((item) => (
                <li key={item.id}>
                  <Link
                    href={adminContentWorkflowRoute(item.id)}
                    className="block rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface)] p-3 hover:border-[var(--adm-accent)]"
                  >
                    <div className="flex items-center justify-between gap-2">
                      <p className="adm-text text-[14px] font-semibold">{item.ideaTitle}</p>
                      <span className="adm-subtle text-[11px]" dir="ltr">
                        {item.currentStep}
                      </span>
                    </div>
                    <p className="adm-subtle mt-1 text-[12px]">
                      وضعیت ایده: {item.ideaStatus} · {formatDateTimeFa(item.updatedAtUtc)}
                    </p>
                  </Link>
                </li>
              ))}
            </ul>
          )}
          <p className="adm-subtle mt-4 text-[12px]">
            ویرایشگر عادی همچنان در{" "}
            <Link className="underline" href={ADMIN_ROUTES.contentNew}>
              ایجاد محتوا
            </Link>{" "}
            در دسترس است.
          </p>
        </AdminPageSection>
      ) : null}
    </div>
  );
}

function Stat({ label, value }: { label: string; value: number }) {
  return (
    <div className="rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface)] p-4">
      <p className="adm-subtle text-[11px]">{label}</p>
      <p className="adm-text mt-1 text-[22px] font-bold">{value}</p>
    </div>
  );
}
