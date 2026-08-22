"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";
import {
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from "react";
import { EditorContent, useEditor, type Editor } from "@tiptap/react";
import type { JSONContent } from "@tiptap/core";
import { useAuth } from "@/components/auth";
import { ADMIN_ROUTES, adminContentArticleRoute } from "@/lib/admin/routes";
import {
  useArticleMetadata,
  useContentSeoAnalysis,
  useCreateContent,
  useUpdateArticleMetadata,
  useUpdateContent,
  useUpdateSeoMetadata,
} from "@/lib/admin/content/content-hooks";
import { previewArticleContent } from "@/lib/admin/content/content-api";
import {
  buildArticlePayload,
  buildSeoPayload,
  hasFormErrors,
  mapSeoForm,
  normalizeContentStatus,
  slugify,
  validateArticleForm,
  validateContentForm,
  validateSeoForm,
} from "@/lib/admin/content/content-mappers";
import {
  CONTENT_LIMITS,
  EMPTY_SEO_FORM,
  type AdminContentDetail,
  type ArticleFormErrors,
  type ContentFormErrors,
  type ContentFormValues,
  type ContentStatusValue,
  type SeoFormErrors,
  type SeoFormValues,
} from "@/lib/admin/content/content-types";
import { clearDraft, loadDraft, saveDraft } from "@/lib/admin/content/editor-draft";
import {
  ARTICLE_CONTENT_FORMAT,
  ARTICLE_EDITOR_VERSION,
  countWords,
  EMPTY_ARTICLE_DOC,
  estimateReadingMinutes,
  extractOutline,
  extractPlainText,
  parseArticleDoc,
  serializeArticleDoc,
} from "@/lib/admin/content/block-editor/document";
import { markdownToTiptapDoc } from "@/lib/admin/content/block-editor/markdown-adapter";
import { filterSlashCommands, type SlashCommandItem } from "@/lib/admin/content/block-editor/slash-items";
import { duplicateSelectedBlock, moveSelectedBlock } from "@/lib/admin/content/block-editor/block-commands";
import { AdminIcon } from "@/components/admin/shared/admin-icons";
import { ContentStatusBadge } from "@/components/admin/content/list/content-status-badge";
import { SaveStatusIndicator, type SaveState } from "@/components/admin/content/editor/save-status";
import { SeoPanel } from "@/components/admin/content/seo/seo-panel";
import { ArticleSettingsPanel } from "@/components/admin/content/workspaces/article/article-settings-panel";
import { WorkflowPanel } from "@/components/admin/content/workflow/workflow-panel";
import { MediaPickerDialog } from "@/components/admin/media/media-picker-dialog";
import type { MediaPickerSelection } from "@/lib/admin/media/media-types";
import { useUploadMediaAsset } from "@/lib/admin/media/media-hooks";
import { validateMediaFile } from "@/lib/admin/media/media-validation";
import { createArticleExtensions, runSlashCommand } from "./extensions";
import { FloatingTextToolbar } from "./floating-text-toolbar";
import { SlashCommandMenu } from "./slash-command-menu";
import { BlockSettingsPanel } from "./block-settings-panel";
import { ArticlePreview, type PreviewDevice } from "./article-preview";
import styles from "./article-block-editor.module.css";

const AUTOSAVE_MS = 3000;

function initialValues(initial: AdminContentDetail | undefined): ContentFormValues {
  if (!initial) {
    return {
      title: "",
      slug: "",
      type: "Article",
      body: "",
      status: "Draft",
      excerpt: "",
      coverImage: "",
    };
  }
  return {
    title: initial.title,
    slug: initial.slug,
    type: "Article",
    body: initial.body,
    status: initial.status,
    excerpt: initial.excerpt,
    coverImage: initial.coverImage,
  };
}

function initialDoc(initial: AdminContentDetail | undefined, recoveredJson?: string): JSONContent {
  const recovered = parseArticleDoc(recoveredJson);
  if (recovered) return recovered;
  const stored = parseArticleDoc(initial?.contentJson);
  if (stored) return stored;
  if (initial?.body?.trim()) return markdownToTiptapDoc(initial.body);
  return EMPTY_ARTICLE_DOC;
}

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

type PickerTarget = "cover" | "og" | "image" | "gallery";

export function ArticleBlockEditor({ initial }: { initial?: AdminContentDetail }) {
  const router = useRouter();
  const { token } = useAuth();
  const create = useCreateContent();
  const update = useUpdateContent();
  const seoMutation = useUpdateSeoMetadata();
  const articleMutation = useUpdateArticleMetadata();
  const upload = useUploadMediaAsset();
  const contentId = initial?.id ?? null;
  const draftKey = contentId ?? "new-article";
  const seoAnalysis = useContentSeoAnalysis(contentId);
  const articleMeta = useArticleMetadata(contentId);
  const autoRerunAfterSaveRef = useRef(false);
  const savingRef = useRef(false);
  const pendingAutosaveRef = useRef(false);
  const editorRef = useRef<Editor | null>(null);
  const canvasRef = useRef<HTMLDivElement>(null);
  const persistRef = useRef<(options: { autosave: boolean }) => Promise<void>>(async () => undefined);
  const uploadFilesRef = useRef<(files: File[]) => Promise<void>>(async () => undefined);
  const draftReadyRef = useRef(false);

  const [values, setValues] = useState<ContentFormValues>(() => initialValues(initial));
  const [errors, setErrors] = useState<ContentFormErrors>({});
  const [seo, setSeo] = useState<SeoFormValues>(() => initial?.seo ?? { ...EMPTY_SEO_FORM });
  const [seoErrors, setSeoErrors] = useState<SeoFormErrors>({});
  const [articleErrors, setArticleErrors] = useState<ArticleFormErrors>({});
  const [status, setStatus] = useState<ContentStatusValue>(initial?.status ?? "Draft");
  const [contentSave, setContentSave] = useState<SaveState>("idle");
  const [seoSave, setSeoSave] = useState<SaveState>("idle");
  const [articleSave, setArticleSave] = useState<SaveState>("idle");
  const [draftRecovered, setDraftRecovered] = useState(false);
  const [centerTab, setCenterTab] = useState<"edit" | "preview">("edit");
  const [previewHtml, setPreviewHtml] = useState(initial?.contentHtml ?? "");
  const [previewDevice, setPreviewDevice] = useState<PreviewDevice>("desktop");
  const [pickerTarget, setPickerTarget] = useState<PickerTarget | null>(null);
  const [settingsOpen, setSettingsOpen] = useState(false);
  const [uploadError, setUploadError] = useState<string | null>(null);
  const [lastSavedAt, setLastSavedAt] = useState<string | null>(initial?.lastAutosavedAtUtc ?? initial?.updatedAtUtc ?? null);
  const [slash, setSlash] = useState<{ items: SlashCommandItem[]; index: number; left: number; top: number; from: number; to: number } | null>(null);
  const [toolbar, setToolbar] = useState<{ left: number; top: number } | null>(null);
  const [selectionEpoch, setSelectionEpoch] = useState(0);

  const savedSnapshot = useRef("");

  const editor = useEditor({
    immediatelyRender: false,
    extensions: createArticleExtensions(),
    content: initialDoc(initial),
    editorProps: {
      attributes: {
        dir: "rtl",
        lang: "fa",
        class: "tiptap",
      },
      handlePaste: (_view, event) => {
        const files = event.clipboardData?.files;
        if (files && files.length > 0) {
          void uploadFilesRef.current(Array.from(files));
          return true;
        }
        return false;
      },
      handleDrop: (_view, event) => {
        const files = event.dataTransfer?.files;
        if (files && files.length > 0) {
          event.preventDefault();
          void uploadFilesRef.current(Array.from(files));
          return true;
        }
        return false;
      },
      handleKeyDown: (_view, event) => {
        const current = editorRef.current;
        if (!current) return false;
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "z") return false;
        if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "s") {
          event.preventDefault();
          void persistRef.current({ autosave: false });
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
          applySlash(items[slash?.index ?? 0] ?? items[0], slashState.from, slashState.to);
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
      const plain = extractPlainText(json);
      setValues((prev) => ({ ...prev, body: plain }));
      setContentSave((prev) => (prev === "saving" ? prev : "unsaved"));
      seoAnalysis.markStale();
    },
  });

  editorRef.current = editor;

  useEffect(() => {
    if (!editor || draftReadyRef.current) return;
    const draft = loadDraft(draftKey);
    if (draft) {
      setValues((prev) => ({
        ...prev,
        title: draft.title,
        body: draft.body,
        excerpt: draft.excerpt,
      }));
      const recoveredDoc = parseArticleDoc(draft.contentJson);
      if (recoveredDoc) editor.commands.setContent(recoveredDoc);
      setDraftRecovered(true);
      setContentSave("unsaved");
    }
    draftReadyRef.current = true;
  }, [draftKey, editor]);

  useEffect(() => {
    if (!editor) return;
    const refreshUi = () => {
      setSelectionEpoch((value) => value + 1);
      const slashState = getSlashState(editor);
      if (slashState) {
        const coords = editor.view.coordsAtPos(slashState.to);
        const canvas = canvasRef.current?.getBoundingClientRect();
        setSlash({
          items: filterSlashCommands(slashState.query),
          index: 0,
          from: slashState.from,
          to: slashState.to,
          left: coords.left - (canvas?.left ?? 0),
          top: coords.bottom - (canvas?.top ?? 0) + 8,
        });
      } else {
        setSlash(null);
      }
      const { empty, from } = editor.state.selection;
      if (empty || !editor.view.hasFocus()) {
        setToolbar(null);
        return;
      }
      const coords = editor.view.coordsAtPos(from);
      const canvas = canvasRef.current?.getBoundingClientRect();
      setToolbar({
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

  const documentJson = editor?.getJSON() ?? EMPTY_ARTICLE_DOC;
  const wordCount = countWords(extractPlainText(documentJson));
  const readingTime = estimateReadingMinutes(wordCount);
  const outline = extractOutline(documentJson);

  useEffect(() => {
    savedSnapshot.current = JSON.stringify({
      values: initialValues(initial),
      json: serializeArticleDoc(initialDoc(initial)),
    });
  }, [initial]);

  const isDirty = useMemo(() => {
    const current = JSON.stringify({ values, json: serializeArticleDoc(documentJson) });
    return current !== savedSnapshot.current;
  }, [values, documentJson]);

  useEffect(() => {
    if (!draftReadyRef.current) return;
    saveDraft({
      contentId: draftKey,
      title: values.title,
      body: values.body,
      excerpt: values.excerpt,
      contentJson: serializeArticleDoc(documentJson),
    });
  }, [draftKey, values.title, values.body, values.excerpt, documentJson]);

  useEffect(() => {
    const onLeave = (event: BeforeUnloadEvent) => {
      if (!isDirty) return;
      event.preventDefault();
      event.returnValue = "";
    };
    window.addEventListener("beforeunload", onLeave);
    return () => window.removeEventListener("beforeunload", onLeave);
  }, [isDirty]);

  const persist = useCallback(
    async ({ autosave }: { autosave: boolean }) => {
      const json = editorRef.current?.getJSON() ?? documentJson;
      const serialized = serializeArticleDoc(json);
      const plain = extractPlainText(json).trim() || values.body;
      const payload = {
        title: values.title.trim(),
        slug: values.slug.trim(),
        type: "Article" as const,
        body: plain,
        excerpt: values.excerpt.trim() ? values.excerpt.trim() : null,
        coverImage: values.coverImage.trim() ? values.coverImage.trim() : null,
        contentJson: serialized,
        contentFormat: ARTICLE_CONTENT_FORMAT,
        editorVersion: ARTICLE_EDITOR_VERSION,
        autosave,
      };

      if (!autosave) {
        const validation = validateContentForm({ ...values, body: plain });
        setErrors(validation);
        if (hasFormErrors(validation)) {
          setContentSave("error");
          return;
        }
        if (!contentId) {
          const articleValidation = validateArticleForm(articleMeta.values);
          setArticleErrors(articleValidation);
          if (hasFormErrors(articleValidation)) {
            setContentSave("error");
            return;
          }
        }
      }

      if (savingRef.current) {
        pendingAutosaveRef.current = true;
        return;
      }
      savingRef.current = true;
      setContentSave("saving");
      try {
        if (!contentId) {
          const created = await create.create({
            title: payload.title,
            slug: payload.slug,
            type: "Article",
            body: payload.body,
            status: "Draft",
          });
          await update.run(created.id, payload);
          if (Object.values(seo).some((value) => value.trim())) {
            await seoMutation.run(created.id, buildSeoPayload(seo));
          }
          await articleMutation.run(created.id, buildArticlePayload(articleMeta.values));
          clearDraft();
          setContentSave("saved");
          router.replace(adminContentArticleRoute(created.id));
          router.refresh();
          return;
        }

        const detail = await update.run(contentId, payload);
        setStatus(normalizeContentStatus(detail.contentStatus));
        savedSnapshot.current = JSON.stringify({ values, json: serialized });
        setLastSavedAt(detail.lastAutosavedAtUtc ?? detail.updatedAtUtc);
        if (!autosave) {
          clearDraft();
          setDraftRecovered(false);
        }
        setContentSave("saved");
        if (seoAnalysis.report) autoRerunAfterSaveRef.current = true;
        seoAnalysis.markStale();
      } catch {
        setContentSave("error");
      } finally {
        savingRef.current = false;
        if (pendingAutosaveRef.current) {
          pendingAutosaveRef.current = false;
          void persist({ autosave: true });
        }
      }
    },
    [
      articleMeta.values,
      articleMutation,
      contentId,
      create,
      documentJson,
      router,
      seo,
      seoAnalysis,
      seoMutation,
      update,
      values,
    ],
  );

  useEffect(() => {
    if (!contentId || !isDirty || contentSave === "saving") return;
    const timer = window.setTimeout(() => {
      void persist({ autosave: true });
    }, AUTOSAVE_MS);
    return () => window.clearTimeout(timer);
  }, [contentId, isDirty, contentSave, persist, values, documentJson]);

  const uploadFiles = useCallback(
    async (files: File[]) => {
      setUploadError(null);
      for (const file of files) {
        const check = validateMediaFile(file);
        if (!check.valid) {
          setUploadError(check.error);
          continue;
        }
        try {
          const asset = await upload.upload({ file, altText: file.name, caption: null });
          editorRef.current
            ?.chain()
            .focus()
            .insertContent({
              type: "image",
              attrs: {
                src: asset.absoluteUrl,
                alt: asset.altText || file.name,
                caption: asset.caption || "",
                align: "center",
              },
            })
            .run();
        } catch {
          setUploadError("بارگذاری تصویر ناموفق بود. دوباره تلاش کنید.");
        }
      }
    },
    [upload],
  );
  persistRef.current = persist;
  uploadFilesRef.current = uploadFiles;

  const applySlash = useCallback(
    (item: SlashCommandItem, from: number, to: number) => {
      const instance = editorRef.current;
      if (!instance) return;
      instance.chain().focus().deleteRange({ from, to }).run();
      setSlash(null);
      if (item.command === "image") {
        setPickerTarget("image");
        return;
      }
      if (item.command === "gallery") {
        setPickerTarget("gallery");
        return;
      }
      if (item.command === "youtube") {
        const src = window.prompt("نشانی ویدیوی یوتیوب");
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
    },
    [],
  );

  const handleMediaSelect = useCallback(
    (selection: MediaPickerSelection) => {
      const url = selection.absoluteUrl;
      if (pickerTarget === "cover") {
        setValues((prev) => ({ ...prev, coverImage: url }));
        setContentSave("unsaved");
        if (!seo.ogImage.trim()) setSeo((prev) => ({ ...prev, ogImage: url }));
      } else if (pickerTarget === "og") {
        setSeo((prev) => ({ ...prev, ogImage: url }));
        setSeoSave("unsaved");
        if (!values.coverImage.trim()) setValues((prev) => ({ ...prev, coverImage: url }));
      } else if (pickerTarget === "image") {
        editorRef.current
          ?.chain()
          .focus()
          .insertContent({
            type: "image",
            attrs: { src: url, alt: selection.altText, caption: "", align: "center" },
          })
          .run();
      } else if (pickerTarget === "gallery") {
        const current = editorRef.current;
        if (current?.isActive("gallery")) {
          const items = (current.getAttributes("gallery").items ?? []) as Array<{ src: string; alt?: string }>;
          current.chain().focus().updateAttributes("gallery", { items: [...items, { src: url, alt: selection.altText }] }).run();
        } else {
          current
            ?.chain()
            .focus()
            .insertContent({ type: "gallery", attrs: { items: [{ src: url, alt: selection.altText }] } })
            .run();
        }
      }
      setPickerTarget(null);
    },
    [pickerTarget, seo.ogImage, values.coverImage],
  );

  const loadPreview = useCallback(async () => {
    if (!token) return;
    try {
      const preview = await previewArticleContent(token, {
        contentJson: serializeArticleDoc(editorRef.current?.getJSON() ?? documentJson),
        body: values.body,
      });
      setPreviewHtml(preview.html);
    } catch {
      setPreviewHtml("");
    }
  }, [documentJson, token, values.body]);

  useEffect(() => {
    if (centerTab !== "preview") return;
    void loadPreview();
  }, [centerTab, loadPreview]);

  const saveSeo = useCallback(async () => {
    if (!contentId) return;
    const validation = validateSeoForm(seo);
    setSeoErrors(validation);
    if (hasFormErrors(validation)) {
      setSeoSave("error");
      return;
    }
    setSeoSave("saving");
    try {
      const detail = await seoMutation.run(contentId, buildSeoPayload(seo));
      setSeo(mapSeoForm(detail));
      setStatus(normalizeContentStatus(detail.contentStatus));
      setSeoSave("saved");
    } catch {
      setSeoSave("error");
    }
  }, [contentId, seo, seoMutation]);

  const saveArticle = useCallback(async () => {
    if (!contentId) return;
    const validation = validateArticleForm(articleMeta.values);
    setArticleErrors(validation);
    if (hasFormErrors(validation)) {
      setArticleSave("error");
      return;
    }
    setArticleSave("saving");
    try {
      const saved = await articleMutation.run(contentId, buildArticlePayload(articleMeta.values));
      articleMeta.replaceValues({
        categoryId: saved.categoryId ?? "",
        difficultyLevel:
          saved.difficultyLevel === "Intermediate" || saved.difficultyLevel === "Advanced"
            ? saved.difficultyLevel
            : "Beginner",
        readingTimeMinutes: String(saved.readingTimeMinutes),
        isFeatured: saved.isFeatured,
        allowComments: saved.allowComments,
        tableOfContentsEnabled: saved.tableOfContentsEnabled,
      });
      setArticleSave("saved");
    } catch {
      setArticleSave("error");
    }
  }, [articleMeta, articleMutation, contentId]);

  useEffect(() => {
    if (!autoRerunAfterSaveRef.current) return;
    if (seoAnalysis.status !== "stale") return;
    autoRerunAfterSaveRef.current = false;
    seoAnalysis.analyze();
  }, [seoAnalysis]);

  const onChange = (patch: Partial<ContentFormValues>) => {
    setValues((prev) => ({ ...prev, ...patch }));
    setContentSave((prev) => (prev === "saving" ? prev : "unsaved"));
  };

  const settings = (
    <div className="space-y-4" data-selection-epoch={selectionEpoch}>
      {editor ? <BlockSettingsPanel editor={editor} /> : null}
      <section className="space-y-2">
        <h2 className="adm-text text-[14px] font-bold">نمای کلی</h2>
        {outline.length === 0 ? (
          <p className="adm-subtle text-[12px]">هنوز عنوانی در متن نیست.</p>
        ) : (
          <ul className="space-y-1">
            {outline.map((item) => (
              <li key={item.id} className="adm-text text-[12px]" style={{ paddingInlineStart: (item.level - 2) * 12 }}>
                {item.text}
              </li>
            ))}
          </ul>
        )}
      </section>
      <ArticleSettingsPanel
        values={articleMeta.values}
        errors={articleErrors}
        onChange={articleMeta.setValues}
        onSave={() => void saveArticle()}
        saveState={articleSave}
        error={articleMutation.error ?? articleMeta.error}
        disabled={articleMutation.submitting}
        saveDisabled={!contentId}
        loading={articleMeta.loading}
      />
      <SeoPanel
        values={seo}
        errors={seoErrors}
        onChange={(patch) => {
          setSeo((prev) => ({ ...prev, ...patch }));
          setSeoSave((prev) => (prev === "saving" ? prev : "unsaved"));
        }}
        onSave={() => void saveSeo()}
        saveState={seoSave}
        error={seoMutation.error}
        disabled={!contentId || seoMutation.submitting}
        contentTitle={values.title}
        excerpt={values.excerpt}
        coverImage={values.coverImage}
        slug={values.slug}
        analysisStatus={seoAnalysis.status}
        analysisReport={seoAnalysis.report}
        analysisError={seoAnalysis.error}
        onAnalyze={seoAnalysis.analyze}
        onPickOgImage={() => setPickerTarget("og")}
      />
    </div>
  );

  return (
    <div className={styles.shell} dir="rtl">
      <header className={styles.topBar}>
        <div className="flex flex-wrap items-center gap-2">
          <Link href={ADMIN_ROUTES.contentArticles} className="adm-btn adm-btn-ghost adm-focus inline-flex items-center gap-1">
            <AdminIcon name="chevron" size={16} />
            بازگشت
          </Link>
          <ContentStatusBadge status={status} />
          <SaveStatusIndicator state={contentSave} />
          <span className={styles.topMeta}>
            {wordCount.toLocaleString("fa-IR")} واژه · حدود {readingTime.toLocaleString("fa-IR")} دقیقه مطالعه
            {lastSavedAt ? ` · آخرین ذخیره ${new Date(lastSavedAt).toLocaleTimeString("fa-IR")}` : null}
          </span>
        </div>
        <div className="flex flex-wrap items-center gap-2">
          <button type="button" className={`adm-btn adm-focus ${styles.drawerToggle}`} onClick={() => setSettingsOpen(true)}>
            تنظیمات
          </button>
          <button
            type="button"
            className="adm-btn adm-btn-ghost adm-focus"
            onClick={() => setCenterTab((tab) => (tab === "edit" ? "preview" : "edit"))}
          >
            {centerTab === "edit" ? "پیش‌نمایش" : "ویرایش"}
          </button>
          <button
            type="button"
            className="adm-btn adm-btn-outline adm-focus"
            disabled={create.submitting || update.submitting}
            onClick={() => void persist({ autosave: false })}
          >
            ذخیره پیش‌نویس
          </button>
          <button
            type="button"
            className="adm-btn adm-btn-primary adm-focus"
            disabled={create.submitting || update.submitting}
            onClick={() => void persist({ autosave: false })}
          >
            {contentId ? "به‌روزرسانی" : "ایجاد مقاله"}
          </button>
        </div>
      </header>

      {contentId && initial ? (
        <div className="px-4 pt-3">
          <WorkflowPanel contentId={contentId} authorId={initial.authorId} status={status} onStatusChange={setStatus} compact />
        </div>
      ) : (
        <p className="px-4 pt-3 text-[12px] font-semibold text-[var(--adm-warning)]">
          برای فعال‌شدن گردش کار، ابتدا مقاله را ذخیره کنید.
        </p>
      )}

      {draftRecovered ? (
        <div className="mx-4 mt-3 flex flex-wrap items-center justify-between gap-2 rounded-lg border border-[var(--adm-warning-soft)] bg-[var(--adm-warning-soft)] px-3 py-2">
          <span className="text-[12px] font-semibold text-[var(--adm-warning)]">پیش‌نویس محلی بازیابی شد.</span>
          <button
            type="button"
            className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]"
            onClick={() => {
              clearDraft();
              setDraftRecovered(false);
              editor?.commands.setContent(initialDoc(initial));
              setValues(initialValues(initial));
              setContentSave("idle");
            }}
          >
            دور انداختن
          </button>
        </div>
      ) : null}

      <div className={styles.layout}>
        <div className={styles.canvasWrap}>
          {centerTab === "preview" ? (
            <div className="space-y-3">
              <div className="flex gap-2">
                {(["desktop", "tablet", "mobile"] as const).map((device) => (
                  <button
                    key={device}
                    type="button"
                    className={`adm-btn adm-focus px-3 py-1 text-[11px] ${previewDevice === device ? "adm-btn-primary" : "adm-btn-ghost"}`}
                    onClick={() => setPreviewDevice(device)}
                  >
                    {device === "desktop" ? "دسکتاپ" : device === "tablet" ? "تبلت" : "موبایل"}
                  </button>
                ))}
              </div>
              <ArticlePreview html={previewHtml} device={previewDevice} />
            </div>
          ) : (
            <div className={styles.canvas} ref={canvasRef} style={{ position: "relative" }}>
              <input
                className={styles.titleInput}
                value={values.title}
                maxLength={200}
                placeholder="عنوان مقاله"
                onChange={(event) => {
                  const title = event.target.value;
                  onChange({
                    title,
                    slug: contentId || values.slug ? values.slug : slugify(title),
                  });
                }}
                aria-invalid={Boolean(errors.title)}
              />
              {errors.title ? <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{errors.title}</p> : null}
              <div className="mb-4 mt-3 grid gap-3 sm:grid-cols-2">
                <label className="space-y-1.5">
                  <span className="adm-text text-[12px] font-semibold">اسلاگ</span>
                  <input
                    className="adm-input text-start"
                    dir="ltr"
                    value={values.slug}
                    maxLength={300}
                    onChange={(event) => onChange({ slug: event.target.value })}
                    aria-invalid={Boolean(errors.slug)}
                  />
                </label>
                <label className="space-y-1.5">
                  <span className="adm-text text-[12px] font-semibold">تصویر کاور</span>
                  <div className="flex gap-1.5">
                    <input
                      className="adm-input min-w-0 flex-1 text-start"
                      dir="ltr"
                      value={values.coverImage}
                      onChange={(event) => onChange({ coverImage: event.target.value })}
                    />
                    <button type="button" className="adm-btn adm-btn-ghost adm-focus" onClick={() => setPickerTarget("cover")}>
                      رسانه
                    </button>
                  </div>
                </label>
              </div>
              <label className="mb-4 block space-y-1.5">
                <span className="adm-text text-[12px] font-semibold">خلاصه</span>
                <textarea
                  className="adm-input min-h-[72px]"
                  value={values.excerpt}
                  maxLength={CONTENT_LIMITS.excerpt}
                  onChange={(event) => onChange({ excerpt: event.target.value })}
                />
              </label>
              <div className="mb-2 flex flex-wrap gap-1.5">
                <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => editor?.chain().focus().undo().run()}>
                  واگرد
                </button>
                <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => editor?.chain().focus().redo().run()}>
                  انجام مجدد
                </button>
                <button
                  type="button"
                  className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]"
                  onClick={() => {
                    const slashState = editor ? getSlashState(editor) : null;
                    if (slashState) {
                      setSlash({
                        items: filterSlashCommands(""),
                        index: 0,
                        from: slashState.from,
                        to: slashState.to,
                        left: 16,
                        top: 120,
                      });
                      return;
                    }
                    editor?.chain().focus().insertContent("/").run();
                  }}
                >
                  + بلوک
                </button>
                <button type="button" className="adm-btn adm-btn-ghost adm-focus px-2 py-1 text-[11px]" onClick={() => setPickerTarget("image")}>
                  تصویر
                </button>
              </div>
              <div className={styles.editorRoot}>
                <EditorContent editor={editor} />
              </div>
              {upload.submitting ? <p className="adm-subtle mt-2 text-[12px]">در حال بارگذاری تصویر…</p> : null}
              {uploadError ? (
                <div className={styles.uploadError}>
                  {uploadError}
                  <button type="button" className="adm-btn adm-btn-ghost adm-focus ms-2 px-2 py-0.5 text-[11px]" onClick={() => setUploadError(null)}>
                    بستن
                  </button>
                </div>
              ) : null}
              {errors.body ? <p className="mt-2 text-[11px] font-semibold text-[var(--adm-danger)]">{errors.body}</p> : null}
              {toolbar && editor ? <FloatingTextToolbar editor={editor} left={toolbar.left} top={toolbar.top} /> : null}
              {slash ? (
                <SlashCommandMenu
                  items={slash.items}
                  activeIndex={slash.index}
                  left={slash.left}
                  top={slash.top}
                  onSelect={(item) => applySlash(item, slash.from, slash.to)}
                />
              ) : null}
            </div>
          )}
        </div>
        <aside className={`${styles.sidebar} hidden min-[1100px]:block`}>{settings}</aside>
      </div>

      {settingsOpen ? (
        <div className={styles.mobileDrawer} onMouseDown={() => setSettingsOpen(false)}>
          <div className={styles.mobileDrawerPanel} onMouseDown={(event) => event.stopPropagation()}>
            <div className="mb-3 flex justify-between">
              <strong className="adm-text">تنظیمات مقاله</strong>
              <button type="button" className="adm-btn adm-btn-ghost adm-focus" onClick={() => setSettingsOpen(false)}>
                بستن
              </button>
            </div>
            {settings}
          </div>
        </div>
      ) : null}

      <MediaPickerDialog
        open={pickerTarget !== null}
        onClose={() => setPickerTarget(null)}
        onSelect={handleMediaSelect}
        title={pickerTarget === "cover" ? "انتخاب تصویر کاور" : "انتخاب رسانه"}
      />
    </div>
  );
}
