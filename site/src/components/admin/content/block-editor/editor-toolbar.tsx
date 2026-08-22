"use client";

import type { Editor } from "@tiptap/react";
import { CODE_LANGUAGES } from "@/lib/admin/content/block-editor/code-languages";
import styles from "./article-rich-text-editor.module.css";

type EditorToolbarProps = {
  editor: Editor;
  disabled?: boolean;
  moreOpen: boolean;
  onToggleMore: () => void;
  onPreview?: () => void;
  onFullscreen: () => void;
  fullscreen: boolean;
  onOpenLink: () => void;
  onOpenImage: () => void;
  onInsertTable: () => void;
  onInsertCallout: (variant: "info" | "warning" | "success" | "tip") => void;
};

const TEXT_COLORS = [
  { label: "پیش‌فرض", value: "" },
  { label: "قرمز", value: "#e11d48" },
  { label: "آبی", value: "#2563eb" },
  { label: "سبز", value: "#059669" },
  { label: "نارنجی", value: "#d97706" },
  { label: "بنفش", value: "#7c3aed" },
];

export function EditorToolbar({
  editor,
  disabled,
  moreOpen,
  onToggleMore,
  onPreview,
  onFullscreen,
  fullscreen,
  onOpenLink,
  onOpenImage,
  onInsertTable,
  onInsertCallout,
}: EditorToolbarProps) {
  return (
    <div className={styles.toolbar} role="toolbar" aria-label="نوار ابزار ویرایشگر مقاله">
      <div className={styles.toolbarGroup}>
        <ToolBtn label="واگرد" shortcut="Ctrl+Z" disabled={disabled} onClick={() => editor.chain().focus().undo().run()}>↶</ToolBtn>
        <ToolBtn label="انجام مجدد" shortcut="Ctrl+Shift+Z" disabled={disabled} onClick={() => editor.chain().focus().redo().run()}>↷</ToolBtn>
      </div>
      <div className={styles.toolbarGroup}>
        <ToolBtn label="پاراگراف" active={editor.isActive("paragraph")} disabled={disabled} onClick={() => editor.chain().focus().setParagraph().run()}>P</ToolBtn>
        <ToolBtn label="عنوان ۲" active={editor.isActive("heading", { level: 2 })} disabled={disabled} onClick={() => editor.chain().focus().toggleHeading({ level: 2 }).run()}>H2</ToolBtn>
        <ToolBtn label="عنوان ۳" active={editor.isActive("heading", { level: 3 })} disabled={disabled} onClick={() => editor.chain().focus().toggleHeading({ level: 3 }).run()}>H3</ToolBtn>
        <ToolBtn label="عنوان ۴" active={editor.isActive("heading", { level: 4 })} disabled={disabled} onClick={() => editor.chain().focus().toggleHeading({ level: 4 }).run()}>H4</ToolBtn>
      </div>
      <div className={styles.toolbarGroup}>
        <ToolBtn label="ضخیم" shortcut="Ctrl+B" active={editor.isActive("bold")} disabled={disabled} onClick={() => editor.chain().focus().toggleBold().run()}><b>B</b></ToolBtn>
        <ToolBtn label="کج" shortcut="Ctrl+I" active={editor.isActive("italic")} disabled={disabled} onClick={() => editor.chain().focus().toggleItalic().run()}><i>I</i></ToolBtn>
        <ToolBtn label="زیرخط" shortcut="Ctrl+U" active={editor.isActive("underline")} disabled={disabled} onClick={() => editor.chain().focus().toggleUnderline().run()}><u>U</u></ToolBtn>
        <ToolBtn label="خط‌خورده" active={editor.isActive("strike")} disabled={disabled} onClick={() => editor.chain().focus().toggleStrike().run()}><s>S</s></ToolBtn>
        <ToolBtn label="هایلایت" active={editor.isActive("highlight")} disabled={disabled} onClick={() => editor.chain().focus().toggleHighlight().run()}>ح</ToolBtn>
        <label className={styles.toolBtn} title="رنگ متن">
          <span className="sr-only">رنگ متن</span>
          <input
            className={styles.colorInput}
            type="color"
            aria-label="رنگ متن"
            disabled={disabled}
            value={normalizeColor(editor.getAttributes("textStyle").color)}
            onChange={(event) => {
              const value = event.target.value;
              if (!value) editor.chain().focus().unsetColor().run();
              else editor.chain().focus().setColor(value).run();
            }}
          />
        </label>
        <select
          className="adm-input h-8 min-w-[4.5rem] py-0 text-[11px]"
          aria-label="رنگ متن"
          disabled={disabled}
          value={String(editor.getAttributes("textStyle").color ?? "")}
          onChange={(event) => {
            const value = event.target.value;
            if (!value) editor.chain().focus().unsetColor().run();
            else editor.chain().focus().setColor(value).run();
          }}
        >
          {TEXT_COLORS.map((color) => (
            <option key={color.label} value={color.value}>
              {color.label}
            </option>
          ))}
        </select>
      </div>
      <div className={styles.toolbarGroup}>
        <ToolBtn label="تراز راست" active={editor.isActive({ textAlign: "right" })} disabled={disabled} onClick={() => editor.chain().focus().setTextAlign("right").run()}>⟸</ToolBtn>
        <ToolBtn label="تراز وسط" active={editor.isActive({ textAlign: "center" })} disabled={disabled} onClick={() => editor.chain().focus().setTextAlign("center").run()}>☰</ToolBtn>
        <ToolBtn label="تراز چپ" active={editor.isActive({ textAlign: "left" })} disabled={disabled} onClick={() => editor.chain().focus().setTextAlign("left").run()}>⟹</ToolBtn>
        <ToolBtn label="تراز دوطرفه" active={editor.isActive({ textAlign: "justify" })} disabled={disabled} onClick={() => editor.chain().focus().setTextAlign("justify").run()}>≣</ToolBtn>
      </div>
      <div className={styles.toolbarGroup}>
        <ToolBtn label="فهرست نشانه‌دار" active={editor.isActive("bulletList")} disabled={disabled} onClick={() => editor.chain().focus().toggleBulletList().run()}>•</ToolBtn>
        <ToolBtn label="فهرست شماره‌دار" active={editor.isActive("orderedList")} disabled={disabled} onClick={() => editor.chain().focus().toggleOrderedList().run()}>1.</ToolBtn>
        <ToolBtn label="چک‌لیست" active={editor.isActive("taskList")} disabled={disabled} onClick={() => editor.chain().focus().toggleTaskList().run()}>☑</ToolBtn>
        <ToolBtn label="نقل‌قول" active={editor.isActive("blockquote")} disabled={disabled} onClick={() => editor.chain().focus().toggleBlockquote().run()}>❝</ToolBtn>
      </div>
      <div className={`${styles.toolbarGroup} ${moreOpen ? "" : "max-[1100px]:hidden"}`}>
        <ToolBtn label="پیوند" shortcut="Ctrl+K" active={editor.isActive("link")} disabled={disabled} onClick={onOpenLink}>🔗</ToolBtn>
        <ToolBtn label="تصویر" disabled={disabled} onClick={onOpenImage}>🖼</ToolBtn>
        <ToolBtn label="جدول" active={editor.isActive("table")} disabled={disabled} onClick={onInsertTable}>▦</ToolBtn>
        <ToolBtn label="بلوک کد" active={editor.isActive("codeBlock")} disabled={disabled} onClick={() => editor.chain().focus().toggleCodeBlock().run()}>{"</>"}</ToolBtn>
        <ToolBtn label="کد درون‌خطی" active={editor.isActive("code")} disabled={disabled} onClick={() => editor.chain().focus().toggleCode().run()}>`</ToolBtn>
        <ToolBtn label="کادر اطلاعات" active={editor.isActive("callout")} disabled={disabled} onClick={() => onInsertCallout("info")}>ℹ</ToolBtn>
        <ToolBtn label="جداکننده" disabled={disabled} onClick={() => editor.chain().focus().setHorizontalRule().run()}>―</ToolBtn>
        <ToolBtn label="پاک‌کردن قالب" disabled={disabled} onClick={() => editor.chain().focus().unsetAllMarks().clearNodes().run()}>Tx</ToolBtn>
        {editor.isActive("codeBlock") ? (
          <select
            className="adm-input h-8 min-w-[7rem] py-0 text-[11px]"
            aria-label="زبان بلوک کد"
            disabled={disabled}
            value={String(editor.getAttributes("codeBlock").language ?? "javascript")}
            onChange={(event) => editor.chain().focus().updateAttributes("codeBlock", { language: event.target.value }).run()}
          >
            {CODE_LANGUAGES.map((language) => (
              <option key={language.id} value={language.id}>
                {language.label}
              </option>
            ))}
          </select>
        ) : null}
      </div>
      <div className={styles.toolbarGroup}>
        <ToolBtn label="موارد بیشتر" active={moreOpen} onClick={onToggleMore}>⋯</ToolBtn>
        <ToolBtn label="پیش‌نمایش" disabled={disabled || !onPreview} onClick={() => onPreview?.()}>👁</ToolBtn>
        <ToolBtn label={fullscreen ? "خروج از تمام‌صفحه" : "تمام‌صفحه"} active={fullscreen} onClick={onFullscreen}>⛶</ToolBtn>
      </div>
    </div>
  );
}

function ToolBtn({
  label,
  shortcut,
  active,
  disabled,
  onClick,
  children,
}: {
  label: string;
  shortcut?: string;
  active?: boolean;
  disabled?: boolean;
  onClick: () => void;
  children: React.ReactNode;
}) {
  return (
    <button
      type="button"
      className={`${styles.toolBtn} ${active ? styles.toolBtnActive : ""}`}
      aria-label={label}
      aria-pressed={active}
      title={shortcut ? `${label} (${shortcut})` : label}
      disabled={disabled}
      onMouseDown={(event) => {
        event.preventDefault();
        onClick();
      }}
    >
      {children}
    </button>
  );
}

function normalizeColor(value: unknown): string {
  return typeof value === "string" && /^#[0-9a-fA-F]{6}$/.test(value) ? value : "#111827";
}
