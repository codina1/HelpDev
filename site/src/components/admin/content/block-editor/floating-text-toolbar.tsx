"use client";

import type { Editor } from "@tiptap/react";
import styles from "./article-block-editor.module.css";

type FloatingTextToolbarProps = {
  editor: Editor;
  left: number;
  top: number;
};

export function FloatingTextToolbar({ editor, left, top }: FloatingTextToolbarProps) {
  return (
    <div className={styles.floating} style={{ left, top }} role="toolbar" aria-label="قالب‌بندی متن">
      <MarkButton editor={editor} action={() => editor.chain().focus().toggleBold().run()} active={editor.isActive("bold")} label="ضخیم" />
      <MarkButton editor={editor} action={() => editor.chain().focus().toggleItalic().run()} active={editor.isActive("italic")} label="کج" />
      <MarkButton editor={editor} action={() => editor.chain().focus().toggleUnderline().run()} active={editor.isActive("underline")} label="زیرخط" />
      <MarkButton editor={editor} action={() => editor.chain().focus().toggleStrike().run()} active={editor.isActive("strike")} label="خط‌خورده" />
      <MarkButton editor={editor} action={() => editor.chain().focus().toggleCode().run()} active={editor.isActive("code")} label="کد" />
      <MarkButton
        editor={editor}
        action={() => {
          const href = window.prompt("نشانی پیوند", editor.getAttributes("link").href ?? "https://");
          if (!href) return;
          if (!/^(https?:\/\/|\/)/i.test(href) || /^javascript:/i.test(href)) {
            window.alert("فقط پیوندهای http(s) یا مسیر داخلی مجاز است.");
            return;
          }
          editor.chain().focus().setLink({ href }).run();
        }}
        active={editor.isActive("link")}
        label="پیوند"
      />
      <MarkButton editor={editor} action={() => editor.chain().focus().setTextAlign("right").run()} active={editor.isActive({ textAlign: "right" })} label="راست" />
      <MarkButton editor={editor} action={() => editor.chain().focus().setTextAlign("center").run()} active={editor.isActive({ textAlign: "center" })} label="وسط" />
      <MarkButton editor={editor} action={() => editor.chain().focus().setTextAlign("left").run()} active={editor.isActive({ textAlign: "left" })} label="چپ" />
    </div>
  );
}

function MarkButton({
  action,
  active,
  label,
}: {
  editor: Editor;
  action: () => void;
  active: boolean;
  label: string;
}) {
  return (
    <button
      type="button"
      className={`adm-btn adm-focus px-2 py-1 text-[11px] ${active ? "adm-btn-primary" : "adm-btn-ghost"}`}
      onMouseDown={(event) => {
        event.preventDefault();
        action();
      }}
    >
      {label}
    </button>
  );
}
