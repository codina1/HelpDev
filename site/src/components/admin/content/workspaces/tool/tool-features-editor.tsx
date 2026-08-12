"use client";

import { useState } from "react";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import type { ToolFeatureDto } from "@/lib/api/content";

type ToolFeaturesEditorProps = {
  features: ToolFeatureDto[];
  disabled?: boolean;
  onAdd: (title: string, description: string | null) => Promise<void>;
  onRemove: (featureId: string) => Promise<void>;
};

export function ToolFeaturesEditor({
  features,
  disabled,
  onAdd,
  onRemove,
}: ToolFeaturesEditorProps) {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const submit = async () => {
    if (!title.trim()) {
      setError("عنوان ویژگی الزامی است.");
      return;
    }
    setBusy(true);
    setError(null);
    try {
      await onAdd(title.trim(), description.trim() || null);
      setTitle("");
      setDescription("");
    } catch {
      setError("افزودن ویژگی ناموفق بود.");
    } finally {
      setBusy(false);
    }
  };

  return (
    <AdminSurface className="space-y-3 p-4">
      <h3 className="adm-text text-[13px] font-bold">ویژگی‌ها</h3>
      {features.length === 0 ? (
        <p className="adm-muted text-[12px]">هنوز ویژگی‌ای ثبت نشده است.</p>
      ) : (
        <ul className="space-y-2">
          {features
            .slice()
            .sort((a, b) => a.order - b.order)
            .map((f) => (
              <li
                key={f.id}
                className="flex items-start justify-between gap-3 rounded-lg border border-[var(--adm-border)] px-3 py-2"
              >
                <div>
                  <p className="adm-text text-[12px] font-semibold">{f.title}</p>
                  {f.description ? <p className="adm-muted text-[11px]">{f.description}</p> : null}
                </div>
                <button
                  type="button"
                  className="adm-btn adm-btn-outline adm-focus text-[11px]"
                  disabled={disabled || busy}
                  onClick={() => void onRemove(f.id)}
                >
                  حذف
                </button>
              </li>
            ))}
        </ul>
      )}
      <div className="space-y-2 border-t border-[var(--adm-border)] pt-3">
        <input
          className="adm-input"
          placeholder="عنوان ویژگی"
          value={title}
          disabled={disabled || busy}
          onChange={(e) => setTitle(e.target.value)}
        />
        <textarea
          className="adm-input min-h-[64px]"
          placeholder="توضیح (اختیاری)"
          value={description}
          disabled={disabled || busy}
          onChange={(e) => setDescription(e.target.value)}
        />
        {error ? <p className="text-[12px] text-[var(--adm-danger)]">{error}</p> : null}
        <button
          type="button"
          className="adm-btn adm-btn-primary adm-focus"
          disabled={disabled || busy}
          onClick={() => void submit()}
        >
          افزودن ویژگی
        </button>
      </div>
    </AdminSurface>
  );
}
