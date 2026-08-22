"use client";

import type { Editor } from "@tiptap/react";
import {
  deleteSelectedBlock,
  duplicateSelectedBlock,
  moveSelectedBlock,
  selectedBlockAttrs,
  selectedBlockType,
} from "@/lib/admin/content/block-editor/block-commands";

const BLOCK_LABELS: Record<string, string> = {
  paragraph: "پاراگراف",
  heading: "عنوان",
  blockquote: "نقل‌قول",
  bulletList: "فهرست نقطه‌ای",
  orderedList: "فهرست شماره‌ای",
  taskList: "چک‌لیست",
  codeBlock: "کد",
  terminal: "ترمینال",
  image: "تصویر",
  gallery: "گالری",
  youtube: "ویدیو",
  table: "جدول",
  callout: "کادر راهنما",
  spacer: "فاصله",
  fileDownload: "دانلود فایل",
  cta: "دکمه فراخوان",
  articleLink: "پیوند مقاله",
  horizontalRule: "جداکننده",
};

type BlockSettingsPanelProps = {
  editor: Editor;
};

export function BlockSettingsPanel({ editor }: BlockSettingsPanelProps) {
  const type = selectedBlockType(editor);
  const attrs = selectedBlockAttrs(editor);
  const label = BLOCK_LABELS[type] ?? type;

  return (
    <section className="space-y-3" aria-labelledby="block-settings-heading">
      <h2 id="block-settings-heading" className="adm-text text-[14px] font-bold">
        تنظیمات بلوک: {label}
      </h2>
      <div className="flex flex-wrap gap-1.5">
        <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => moveSelectedBlock(editor, -1)}>
          بالا
        </button>
        <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => moveSelectedBlock(editor, 1)}>
          پایین
        </button>
        <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => duplicateSelectedBlock(editor)}>
          نسخه‌برداری
        </button>
        <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px] text-[var(--adm-danger)]" onClick={() => deleteSelectedBlock(editor)}>
          حذف
        </button>
      </div>

      {type === "heading" ? (
        <label className="block space-y-1.5">
          <span className="adm-text text-[12px] font-semibold">سطح عنوان</span>
          <select
            className="adm-input"
            value={String(attrs.level ?? 2)}
            onChange={(event) => editor.chain().focus().updateAttributes("heading", { level: Number(event.target.value) }).run()}
          >
            <option value="2">H2</option>
            <option value="3">H3</option>
            <option value="4">H4</option>
          </select>
        </label>
      ) : null}

      {type === "callout" ? (
        <label className="block space-y-1.5">
          <span className="adm-text text-[12px] font-semibold">نوع کادر</span>
          <select
            className="adm-input"
            value={String(attrs.variant ?? "info")}
            onChange={(event) => editor.chain().focus().updateAttributes("callout", { variant: event.target.value }).run()}
          >
            <option value="info">اطلاعات</option>
            <option value="warning">هشدار</option>
            <option value="success">موفقیت</option>
            <option value="note">یادداشت</option>
            <option value="tip">راهنمایی</option>
          </select>
        </label>
      ) : null}

      {type === "image" ? (
        <>
          <Field
            label="متن جایگزین"
            value={String(attrs.alt ?? "")}
            onChange={(value) => editor.chain().focus().updateAttributes("image", { alt: value }).run()}
          />
          <Field
            label="عنوان تصویر"
            value={String(attrs.caption ?? "")}
            onChange={(value) => editor.chain().focus().updateAttributes("image", { caption: value }).run()}
          />
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">چینش</span>
            <select
              className="adm-input"
              value={String(attrs.align ?? "center")}
              onChange={(event) => editor.chain().focus().updateAttributes("image", { align: event.target.value }).run()}
            >
              <option value="right">راست</option>
              <option value="center">وسط</option>
              <option value="left">چپ</option>
              <option value="wide">عریض</option>
              <option value="full">تمام‌عرض</option>
            </select>
          </label>
          <Field
            label="پیوند تصویر"
            value={String(attrs.href ?? "")}
            ltr
            onChange={(value) => editor.chain().focus().updateAttributes("image", { href: value || null }).run()}
          />
        </>
      ) : null}

      {type === "spacer" ? (
        <Field
          label="ارتفاع (پیکسل)"
          value={String(attrs.height ?? 32)}
          ltr
          onChange={(value) => editor.chain().focus().updateAttributes("spacer", { height: Number(value) || 32 }).run()}
        />
      ) : null}

      {type === "codeBlock" ? (
        <>
          <Field
            label="زبان"
            value={String(attrs.language ?? "")}
            ltr
            onChange={(value) => editor.chain().focus().updateAttributes("codeBlock", { language: value || null }).run()}
          />
          <label className="flex items-center gap-2 text-[12px]">
            <input
              type="checkbox"
              checked={Boolean(attrs.showLineNumbers)}
              onChange={(event) =>
                editor.chain().focus().updateAttributes("codeBlock", { showLineNumbers: event.target.checked }).run()
              }
            />
            نمایش شماره خط
          </label>
        </>
      ) : null}

      {type === "youtube" ? (
        <Field
          label="نشانی یوتیوب"
          value={String(attrs.src ?? "")}
          ltr
          onChange={(value) => editor.chain().focus().updateAttributes("youtube", { src: value }).run()}
        />
      ) : null}

      {type === "fileDownload" ? (
        <>
          <Field
            label="نشانی فایل"
            value={String(attrs.href ?? "")}
            ltr
            onChange={(value) => editor.chain().focus().updateAttributes("fileDownload", { href: value }).run()}
          />
          <Field
            label="نام فایل"
            value={String(attrs.name ?? "")}
            onChange={(value) => editor.chain().focus().updateAttributes("fileDownload", { name: value }).run()}
          />
        </>
      ) : null}

      {type === "cta" ? (
        <>
          <Field
            label="متن دکمه"
            value={String(attrs.label ?? "")}
            onChange={(value) => editor.chain().focus().updateAttributes("cta", { label: value }).run()}
          />
          <Field
            label="نشانی"
            value={String(attrs.href ?? "")}
            ltr
            onChange={(value) => editor.chain().focus().updateAttributes("cta", { href: value }).run()}
          />
        </>
      ) : null}

      {type === "articleLink" ? (
        <>
          <Field
            label="عنوان مقاله"
            value={String(attrs.title ?? "")}
            onChange={(value) => editor.chain().focus().updateAttributes("articleLink", { title: value }).run()}
          />
          <Field
            label="مسیر /articles/…"
            value={String(attrs.href ?? "")}
            ltr
            onChange={(value) => editor.chain().focus().updateAttributes("articleLink", { href: value }).run()}
          />
        </>
      ) : null}

      {type === "table" ? (
        <div className="flex flex-wrap gap-1.5">
          <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => editor.chain().focus().addRowAfter().run()}>
            ردیف جدید
          </button>
          <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => editor.chain().focus().addColumnAfter().run()}>
            ستون جدید
          </button>
          <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => editor.chain().focus().deleteRow().run()}>
            حذف ردیف
          </button>
          <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => editor.chain().focus().deleteColumn().run()}>
            حذف ستون
          </button>
        </div>
      ) : null}
    </section>
  );
}

function Field({
  label,
  value,
  onChange,
  ltr,
}: {
  label: string;
  value: string;
  onChange: (value: string) => void;
  ltr?: boolean;
}) {
  return (
    <label className="block space-y-1.5">
      <span className="adm-text text-[12px] font-semibold">{label}</span>
      <input className="adm-input" dir={ltr ? "ltr" : undefined} value={value} onChange={(event) => onChange(event.target.value)} />
    </label>
  );
}
