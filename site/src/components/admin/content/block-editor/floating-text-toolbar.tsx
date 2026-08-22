"use client";

import type { Editor } from "@tiptap/react";
import styles from "./article-rich-text-editor.module.css";

type FloatingTextToolbarProps = {
  editor: Editor;
  left: number;
  top: number;
  onOpenLink: () => void;
};

export function FloatingTextToolbar({ editor, left, top, onOpenLink }: FloatingTextToolbarProps) {
  return (
    <div className={styles.floating} style={{ left, top }} role="toolbar" aria-label="قالب‌بندی متن انتخاب‌شده">
      <MarkButton action={() => editor.chain().focus().toggleBold().run()} active={editor.isActive("bold")} label="ضخیم" />
      <MarkButton action={() => editor.chain().focus().toggleItalic().run()} active={editor.isActive("italic")} label="کج" />
      <MarkButton action={() => editor.chain().focus().toggleUnderline().run()} active={editor.isActive("underline")} label="زیرخط" />
      <MarkButton action={() => editor.chain().focus().toggleStrike().run()} active={editor.isActive("strike")} label="خط‌خورده" />
      <MarkButton action={() => editor.chain().focus().toggleCode().run()} active={editor.isActive("code")} label="کد درون‌خطی" />
      <MarkButton action={() => editor.chain().focus().toggleHighlight().run()} active={editor.isActive("highlight")} label="هایلایت" />
      <MarkButton action={onOpenLink} active={editor.isActive("link")} label="پیوند" />
      <MarkButton action={() => editor.chain().focus().unsetLink().run()} active={false} label="حذف پیوند" />
    </div>
  );
}

export function FloatingBlockToolbar({
  editor,
  left,
  top,
  kind,
  onReplaceImage,
}: {
  editor: Editor;
  left: number;
  top: number;
  kind: "image" | "table";
  onReplaceImage?: () => void;
}) {
  if (kind === "table") {
    return (
      <div className={styles.floating} style={{ left, top }} role="toolbar" aria-label="ابزار جدول">
        <MarkButton action={() => editor.chain().focus().addRowAfter().run()} active={false} label="افزودن سطر" />
        <MarkButton action={() => editor.chain().focus().deleteRow().run()} active={false} label="حذف سطر" />
        <MarkButton action={() => editor.chain().focus().addColumnAfter().run()} active={false} label="افزودن ستون" />
        <MarkButton action={() => editor.chain().focus().deleteColumn().run()} active={false} label="حذف ستون" />
        <MarkButton action={() => editor.chain().focus().toggleHeaderRow().run()} active={false} label="ردیف عنوان" />
        <MarkButton action={() => editor.chain().focus().deleteTable().run()} active={false} label="حذف جدول" />
      </div>
    );
  }

  return (
    <div className={styles.floating} style={{ left, top }} role="toolbar" aria-label="ابزار تصویر">
      <MarkButton action={() => editor.chain().focus().updateAttributes("image", { align: "right" }).run()} active={editor.getAttributes("image").align === "right"} label="راست" />
      <MarkButton action={() => editor.chain().focus().updateAttributes("image", { align: "center" }).run()} active={editor.getAttributes("image").align === "center"} label="وسط" />
      <MarkButton action={() => editor.chain().focus().updateAttributes("image", { align: "left" }).run()} active={editor.getAttributes("image").align === "left"} label="چپ" />
      <MarkButton action={() => editor.chain().focus().updateAttributes("image", { align: "full" }).run()} active={editor.getAttributes("image").align === "full"} label="تمام‌عرض" />
      <MarkButton action={() => onReplaceImage?.()} active={false} label="جایگزینی" />
      <MarkButton action={() => editor.chain().focus().deleteSelection().run()} active={false} label="حذف" />
    </div>
  );
}

function MarkButton({
  action,
  active,
  label,
}: {
  action: () => void;
  active: boolean;
  label: string;
}) {
  return (
    <button
      type="button"
      className={`adm-btn adm-focus px-2 py-1 text-[11px] ${active ? "adm-btn-primary" : "adm-btn-ghost"}`}
      aria-label={label}
      title={label}
      onMouseDown={(event) => {
        event.preventDefault();
        action();
      }}
    >
      {label}
    </button>
  );
}
