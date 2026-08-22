"use client";

import {
  forwardRef,
  useCallback,
  useEffect,
  useImperativeHandle,
  useMemo,
  useRef,
  useState,
} from "react";
import { EditorContent, useEditor, type Editor } from "@tiptap/react";
import type { JSONContent } from "@tiptap/core";
import {
  countCharacters,
  countWords,
  estimateReadingMinutes,
  extractPlainText,
  serializeArticleDoc,
} from "@/lib/admin/content/block-editor/document";
import { duplicateSelectedBlock, moveSelectedBlock, selectedBlockType } from "@/lib/admin/content/block-editor/block-commands";
import { filterSlashCommands, type SlashCommandItem } from "@/lib/admin/content/block-editor/slash-items";
import type { SaveState } from "@/components/admin/content/editor/save-status";
import { ARTICLE_EDITOR_PLACEHOLDER, createArticleExtensions, runSlashCommand } from "./extensions";
import { EditorToolbar } from "./editor-toolbar";
import { EditorStatusBar } from "./editor-status-bar";
import { FloatingBlockToolbar, FloatingTextToolbar } from "./floating-text-toolbar";
import { SlashCommandMenu } from "./slash-command-menu";
import { LinkDialog, type LinkDialogValue } from "./link-dialog";
import { ImageInsertDialog, isSafeImageSrc } from "./image-insert-dialog";
import styles from "./article-rich-text-editor.module.css";

export type EditorContent = JSONContent;

export type ArticleRichTextEditorHandle = {
  getJSON: () => JSONContent;
  insertContent: (content: JSONContent) => void;
  getEditor: () => Editor | null;
};

export type ArticleRichTextEditorProps = {
  value: EditorContent;
  onChange: (content: EditorContent) => void;
  disabled?: boolean;
  error?: string;
  placeholder?: string;
  saveState?: SaveState;
  lastSavedAt?: string | null;
  uploading?: boolean;
  uploadError?: string | null;
  onRetryUpload?: () => void;
  onUploadFiles?: (files: File[]) => void | Promise<void>;
  onRequestMediaLibrary?: () => void;
  onPreview?: () => void;
  onSave?: () => void;
  onReady?: () => void;
};

function getSlashState(editor: Editor): { from: number; to: number; query: string } | null {
  const { $from } = editor.state.selection;
  if (!$from.parent.isTextblock) return null;
  const text = $from.parent.textBetween(0, $from.parentOffset, undefined, "\ufffc");
  const match = /(^|\s)\/([^\s]*)$/.exec(text);
  if (!match) return null;
  const slashOffset = match.index + match[1].length;
  return {
    from: $from.start() + slashOffset,
    to: $from.pos,
    query: match[2] ?? "",
  };
}

export const ArticleRichTextEditor = forwardRef<ArticleRichTextEditorHandle, ArticleRichTextEditorProps>(
  function ArticleRichTextEditor(
    {
      value,
      onChange,
      disabled,
      error,
      placeholder = ARTICLE_EDITOR_PLACEHOLDER,
      saveState,
      lastSavedAt,
      uploading,
      uploadError,
      onRetryUpload,
      onUploadFiles,
      onRequestMediaLibrary,
      onPreview,
      onSave,
      onReady,
    },
    ref,
  ) {
    const lastEmitted = useRef(serializeArticleDoc(value));
    const editorRef = useRef<Editor | null>(null);
    const canvasRef = useRef<HTMLDivElement>(null);
    const uploadRef = useRef(onUploadFiles);
    uploadRef.current = onUploadFiles;
    const saveRef = useRef(onSave);
    saveRef.current = onSave;
    const slashApplyRef = useRef<(item: SlashCommandItem, from: number, to: number) => void>(() => undefined);

    const [fullscreen, setFullscreen] = useState(false);
    const [moreOpen, setMoreOpen] = useState(false);
    const [linkOpen, setLinkOpen] = useState(false);
    const [imageOpen, setImageOpen] = useState(false);
    const [slash, setSlash] = useState<{ items: SlashCommandItem[]; index: number; left: number; top: number; from: number; to: number } | null>(null);
    const [toolbar, setToolbar] = useState<{ left: number; top: number; kind: "text" | "image" | "table" } | null>(null);
    const [gutter, setGutter] = useState<{ left: number; top: number; pos: number } | null>(null);
    const [tick, setTick] = useState(0);

    const extensions = useMemo(() => createArticleExtensions(placeholder), [placeholder]);

    const editor = useEditor({
      immediatelyRender: false,
      editable: !disabled,
      extensions,
      content: value,
      editorProps: {
        attributes: {
          dir: "rtl",
          lang: "fa",
          class: "tiptap",
        },
        handlePaste: (_view, event) => {
          const files = event.clipboardData?.files;
          if (files && files.length > 0) {
            void uploadRef.current?.(Array.from(files));
            return true;
          }
          return false;
        },
        handleDrop: (_view, event) => {
          const files = event.dataTransfer?.files;
          if (files && files.length > 0) {
            event.preventDefault();
            void uploadRef.current?.(Array.from(files));
            return true;
          }
          return false;
        },
        handleKeyDown: (_view, event) => {
          const current = editorRef.current;
          if (!current) return false;
          if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "s") {
            event.preventDefault();
            saveRef.current?.();
            return true;
          }
          if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k") {
            event.preventDefault();
            setLinkOpen(true);
            return true;
          }
          if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "d") {
            event.preventDefault();
            duplicateSelectedBlock(current);
            return true;
          }
          if (event.altKey && event.key === "ArrowUp") {
            event.preventDefault();
            moveSelectedBlock(current, -1);
            return true;
          }
          if (event.altKey && event.key === "ArrowDown") {
            event.preventDefault();
            moveSelectedBlock(current, 1);
            return true;
          }
          const slashState = getSlashState(current);
          if (!slashState) return false;
          const items = filterSlashCommands(slashState.query);
          if (event.key === "ArrowDown") {
            event.preventDefault();
            setSlash((prev) => (prev ? { ...prev, index: (prev.index + 1) % Math.max(items.length, 1) } : prev));
            return true;
          }
          if (event.key === "ArrowUp") {
            event.preventDefault();
            setSlash((prev) =>
              prev ? { ...prev, index: (prev.index - 1 + Math.max(items.length, 1)) % Math.max(items.length, 1) } : prev,
            );
            return true;
          }
          if (event.key === "Enter" && items[0]) {
            event.preventDefault();
            slashApplyRef.current(items[slash?.index ?? 0] ?? items[0], slashState.from, slashState.to);
            return true;
          }
          if (event.key === "Escape") {
            setSlash(null);
            return true;
          }
          return false;
        },
      },
      onUpdate: ({ editor: instance }) => {
        const json = instance.getJSON();
        lastEmitted.current = serializeArticleDoc(json);
        onChange(json);
      },
    });

    editorRef.current = editor;

    useEffect(() => {
      if (editor) onReady?.();
    }, [editor, onReady]);

    useImperativeHandle(
      ref,
      () => ({
        getJSON: () => editorRef.current?.getJSON() ?? value,
        insertContent: (content) => {
          editorRef.current?.chain().focus().insertContent(content).run();
        },
        getEditor: () => editorRef.current,
      }),
      [value],
    );

    useEffect(() => {
      if (!editor) return;
      const next = serializeArticleDoc(value);
      if (next === lastEmitted.current) return;
      lastEmitted.current = next;
      editor.commands.setContent(value, { emitUpdate: false });
    }, [editor, value]);

    useEffect(() => {
      editor?.setEditable(!disabled);
    }, [disabled, editor]);

    const applySlash = useCallback((item: SlashCommandItem, from: number, to: number) => {
      const instance = editorRef.current;
      if (!instance) return;
      instance.chain().focus().deleteRange({ from, to }).run();
      setSlash(null);
      if (item.command === "image") {
        setImageOpen(true);
        return;
      }
      if (item.command === "youtube") {
        const src = window.prompt("نشانی ویدئوی یوتیوب");
        if (src) runSlashCommand(instance, "youtube", { src });
        return;
      }
      if (item.command === "fileDownload") {
        const href = window.prompt("نشانی فایل", "/");
        const name = window.prompt("نام فایل", "دانلود فایل") ?? "دانلود فایل";
        if (href) runSlashCommand(instance, "fileDownload", { href, name });
        return;
      }
      if (item.command === "cta") {
        const label = window.prompt("متن دکمه", "ادامه مطلب") ?? "ادامه مطلب";
        const href = window.prompt("نشانی دکمه", "/");
        if (href) runSlashCommand(instance, "cta", { href, label });
        return;
      }
      if (item.command === "articleLink") {
        const slug = window.prompt("اسلاگ مقاله") ?? "";
        const title = window.prompt("عنوان نمایشی", slug) ?? slug;
        const href = slug.startsWith("/") ? slug : `/articles/${slug}`;
        runSlashCommand(instance, "articleLink", { href, title, slug });
        return;
      }
      runSlashCommand(instance, item.command);
    }, []);
    slashApplyRef.current = applySlash;

    useEffect(() => {
      if (!editor) return;
      const refreshUi = () => {
        setTick((value) => value + 1);
        const slashState = getSlashState(editor);
        const canvas = canvasRef.current?.getBoundingClientRect();
        if (slashState) {
          const coords = editor.view.coordsAtPos(slashState.to);
          const left = Math.min(Math.max(8, coords.left - (canvas?.left ?? 0)), (canvas?.width ?? 320) - 280);
          const top = coords.bottom - (canvas?.top ?? 0) + 8;
          setSlash({
            items: filterSlashCommands(slashState.query),
            index: 0,
            from: slashState.from,
            to: slashState.to,
            left,
            top,
          });
        } else {
          setSlash(null);
        }

        const type = selectedBlockType(editor);
        const { empty, from } = editor.state.selection;
        if (!editor.view.hasFocus()) {
          setToolbar(null);
          return;
        }
        if (type === "image") {
          const coords = editor.view.coordsAtPos(from);
          setToolbar({
            kind: "image",
            left: Math.max(8, coords.left - (canvas?.left ?? 0)),
            top: Math.max(8, coords.top - (canvas?.top ?? 0) - 44),
          });
          return;
        }
        if (editor.isActive("table")) {
          const coords = editor.view.coordsAtPos(from);
          setToolbar({
            kind: "table",
            left: Math.max(8, coords.left - (canvas?.left ?? 0)),
            top: Math.max(8, coords.top - (canvas?.top ?? 0) - 44),
          });
          return;
        }
        if (empty) {
          setToolbar(null);
          return;
        }
        const coords = editor.view.coordsAtPos(from);
        setToolbar({
          kind: "text",
          left: Math.max(8, coords.left - (canvas?.left ?? 0)),
          top: Math.max(8, coords.top - (canvas?.top ?? 0) - 44),
        });
      };
      editor.on("transaction", refreshUi);
      editor.on("selectionUpdate", refreshUi);
      return () => {
        editor.off("transaction", refreshUi);
        editor.off("selectionUpdate", refreshUi);
      };
    }, [editor]);

    useEffect(() => {
      const onPointer = (event: MouseEvent) => {
        if (!slash) return;
        const target = event.target as HTMLElement | null;
        if (target?.closest(`.${styles.slash}`)) return;
        setSlash(null);
      };
      document.addEventListener("mousedown", onPointer);
      return () => document.removeEventListener("mousedown", onPointer);
    }, [slash]);

    const insertImage = useCallback(
      (attrs: Record<string, string | number | null>) => {
        editor?.chain().focus().insertContent({ type: "image", attrs: { align: "center", ...attrs } }).run();
        setImageOpen(false);
      },
      [editor],
    );

    const applyLink = useCallback(
      (next: LinkDialogValue) => {
        if (!editor) return;
        const href = next.href.trim();
        if (!isSafeImageSrc(href) && !/^(https?:\/\/|\/|mailto:)/i.test(href)) {
          setLinkOpen(false);
          return;
        }
        const target = next.newTab ? "_blank" : null;
        const rel = next.newTab ? "noopener noreferrer" : null;
        if (next.text.trim() && editor.state.selection.empty) {
          editor
            .chain()
            .focus()
            .insertContent({
              type: "text",
              text: next.text.trim(),
              marks: [{ type: "link", attrs: { href, target, rel, title: next.title || null } }],
            })
            .run();
        } else {
          editor.chain().focus().extendMarkRange("link").setLink({ href, target: target ?? undefined }).run();
          if (next.title) editor.chain().focus().extendMarkRange("link").updateAttributes("link", { title: next.title, rel }).run();
        }
        setLinkOpen(false);
      },
      [editor],
    );

    const documentJson = editor?.getJSON() ?? value;
    const plain = extractPlainText(documentJson);
    const wordCount = countWords(plain);
    const characterCount = countCharacters(plain);
    const readingTime = estimateReadingMinutes(wordCount);
    const linkAttrs = editor?.getAttributes("link") ?? {};
    const selectedText = editor?.state.doc.textBetween(editor.state.selection.from, editor.state.selection.to) ?? "";

    return (
      <div className={`${styles.editor} ${fullscreen ? styles.editorFullscreen : ""}`} data-editor-tick={tick}>
        {editor ? (
          <EditorToolbar
            editor={editor}
            disabled={disabled}
            moreOpen={moreOpen}
            onToggleMore={() => setMoreOpen((open) => !open)}
            onPreview={onPreview}
            onFullscreen={() => setFullscreen((open) => !open)}
            fullscreen={fullscreen}
            onOpenLink={() => setLinkOpen(true)}
            onOpenImage={() => setImageOpen(true)}
            onInsertTable={() => editor.chain().focus().insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run()}
            onInsertCallout={(variant) => runSlashCommand(editor, `callout-${variant}`)}
          />
        ) : null}
        <div
          className={styles.scroll}
          ref={canvasRef}
          onMouseMove={(event) => {
            if (!editor) return;
            const pos = editor.view.posAtCoords({ left: event.clientX, top: event.clientY });
            if (!pos) return;
            const $pos = editor.state.doc.resolve(pos.pos);
            if ($pos.depth < 1) return;
            const after = $pos.after(1);
            const coords = editor.view.coordsAtPos(Math.min(after, editor.state.doc.content.size));
            const canvas = canvasRef.current?.getBoundingClientRect();
            setGutter({
              pos: after,
              left: 4,
              top: coords.top - (canvas?.top ?? 0) - 8,
            });
          }}
        >
          <div className={styles.editorRoot}>
            <EditorContent editor={editor} />
          </div>
          {gutter && !disabled ? (
            <button
              type="button"
              className={styles.gutterBtn}
              style={{ left: gutter.left, top: gutter.top }}
              aria-label="افزودن بلوک میان متن"
              title="افزودن بلوک"
              onMouseDown={(event) => {
                event.preventDefault();
                editor?.chain().focus().insertContentAt(gutter.pos, { type: "paragraph", content: [{ type: "text", text: "/" }] }).run();
              }}
            >
              +
            </button>
          ) : null}
          {toolbar && editor && toolbar.kind === "text" ? (
            <FloatingTextToolbar editor={editor} left={toolbar.left} top={toolbar.top} onOpenLink={() => setLinkOpen(true)} />
          ) : null}
          {toolbar && editor && toolbar.kind !== "text" ? (
            <FloatingBlockToolbar
              editor={editor}
              left={toolbar.left}
              top={toolbar.top}
              kind={toolbar.kind}
              onReplaceImage={() => setImageOpen(true)}
            />
          ) : null}
          {slash ? (
            <SlashCommandMenu
              items={slash.items}
              activeIndex={slash.index}
              left={slash.left}
              top={slash.top}
              onSelect={(item) => applySlash(item, slash.from, slash.to)}
            />
          ) : null}
          {uploading ? (
            <>
              <div className={styles.skeleton} aria-hidden />
              <p className={styles.uploadBanner}>در حال بارگذاری تصویر روی سرور…</p>
            </>
          ) : null}
          {uploadError ? (
            <div className={styles.uploadError}>
              {uploadError}
              {onRetryUpload ? (
                <button type="button" className="adm-btn adm-btn-ghost adm-focus ms-2 px-2 py-0.5 text-[11px]" onClick={onRetryUpload}>
                  تلاش دوباره
                </button>
              ) : null}
            </div>
          ) : null}
          {error ? <p className={styles.errorText}>{error}</p> : null}
        </div>
        <EditorStatusBar
          wordCount={wordCount}
          characterCount={characterCount}
          readingTime={readingTime}
          saveState={saveState}
          lastSavedAt={lastSavedAt}
          fullscreen={fullscreen}
          onFullscreen={() => setFullscreen((open) => !open)}
        />
        <LinkDialog
          open={linkOpen}
          initial={{
            href: String(linkAttrs.href ?? ""),
            text: selectedText,
            title: String(linkAttrs.title ?? ""),
            newTab: linkAttrs.target === "_blank",
          }}
          onClose={() => setLinkOpen(false)}
          onApply={applyLink}
          onRemove={() => {
            editor?.chain().focus().unsetLink().run();
            setLinkOpen(false);
          }}
        />
        <ImageInsertDialog
          open={imageOpen}
          onClose={() => setImageOpen(false)}
          onInsertUrl={(next) =>
            insertImage({
              src: next.src,
              alt: next.alt,
              title: next.title,
              caption: next.caption,
            })
          }
          onPickFile={(file) => {
            setImageOpen(false);
            void onUploadFiles?.([file]);
          }}
          onOpenLibrary={() => {
            setImageOpen(false);
            onRequestMediaLibrary?.();
          }}
        />
      </div>
    );
  },
);
