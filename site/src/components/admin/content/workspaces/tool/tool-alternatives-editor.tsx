"use client";

import { AdminSurface } from "@/components/admin/page/admin-surface";

export type ToolAlternativeDraft = {
  alternativeToolContentId: string;
  order: number;
};

type ToolAlternativesEditorProps = {
  items: ToolAlternativeDraft[];
  onChange: (items: ToolAlternativeDraft[]) => void;
  disabled?: boolean;
};

export function ToolAlternativesEditor({
  items,
  onChange,
  disabled,
}: ToolAlternativesEditorProps) {
  const add = () => {
    onChange([...items, { alternativeToolContentId: "", order: items.length }]);
  };

  const update = (index: number, contentId: string) => {
    onChange(
      items.map((item, i) =>
        i === index ? { ...item, alternativeToolContentId: contentId.trim() } : item,
      ),
    );
  };

  const remove = (index: number) => {
    onChange(items.filter((_, i) => i !== index).map((item, order) => ({ ...item, order })));
  };

  return (
    <AdminSurface className="space-y-3 p-4">
      <h3 className="adm-text text-[13px] font-bold">جایگزین‌ها</h3>
      <p className="adm-muted text-[11px]">
        شناسهٔ Content ابزار جایگزین را وارد کنید (ContentId از نوع Tool).
      </p>
      {items.length === 0 ? (
        <p className="adm-muted text-[12px]">جایگزینی ثبت نشده است.</p>
      ) : (
        <ul className="space-y-2">
          {items.map((item, index) => (
            <li key={`${item.order}-${index}`} className="flex items-center gap-2">
              <input
                className="adm-input flex-1 font-mono text-[12px]"
                dir="ltr"
                placeholder="00000000-0000-0000-0000-000000000000"
                value={item.alternativeToolContentId}
                disabled={disabled}
                onChange={(e) => update(index, e.target.value)}
              />
              <button
                type="button"
                className="adm-btn adm-btn-outline adm-focus text-[11px]"
                disabled={disabled}
                onClick={() => remove(index)}
              >
                حذف
              </button>
            </li>
          ))}
        </ul>
      )}
      <button
        type="button"
        className="adm-btn adm-btn-outline adm-focus"
        disabled={disabled}
        onClick={add}
      >
        افزودن جایگزین
      </button>
    </AdminSurface>
  );
}
