"use client";

import { Node, mergeAttributes } from "@tiptap/core";
import StarterKit from "@tiptap/starter-kit";
import Image from "@tiptap/extension-image";
import Placeholder from "@tiptap/extension-placeholder";
import TextAlign from "@tiptap/extension-text-align";
import Underline from "@tiptap/extension-underline";
import Highlight from "@tiptap/extension-highlight";
import { TextStyle } from "@tiptap/extension-text-style";
import Color from "@tiptap/extension-color";
import Typography from "@tiptap/extension-typography";
import CharacterCount from "@tiptap/extension-character-count";
import { Table, TableCell, TableHeader, TableRow } from "@tiptap/extension-table";
import TaskList from "@tiptap/extension-task-list";
import TaskItem from "@tiptap/extension-task-item";
import Youtube from "@tiptap/extension-youtube";
import CodeBlockLowlight from "@tiptap/extension-code-block-lowlight";
import { common, createLowlight } from "lowlight";
import csharp from "highlight.js/lib/languages/csharp";
import kotlin from "highlight.js/lib/languages/kotlin";
import dart from "highlight.js/lib/languages/dart";
import powershell from "highlight.js/lib/languages/powershell";
import type { Editor } from "@tiptap/react";

const lowlight = createLowlight(common);
lowlight.register("csharp", csharp);
lowlight.register("cs", csharp);
lowlight.register("kotlin", kotlin);
lowlight.register("dart", dart);
lowlight.register("powershell", powershell);

export const ARTICLE_EDITOR_PLACEHOLDER =
  "محتوای مقاله را اینجا بنویسید؛ برای افزودن بلوک جدید / را تایپ کنید...";

const ArticleImage = Image.extend({
  addAttributes() {
    return {
      ...this.parent?.(),
      mediaId: { default: null },
      caption: { default: null },
      title: { default: null },
      align: { default: "center" },
      width: { default: null },
      height: { default: null },
      href: { default: null },
      target: { default: null },
    };
  },
  renderHTML({ HTMLAttributes }) {
    const { caption, align, href, target, mediaId, ...imgAttrs } = HTMLAttributes as Record<string, string>;
    const image = [
      "img",
      mergeAttributes(imgAttrs, {
        class: "hd-editor-image",
        loading: "lazy",
        "data-media-id": mediaId || null,
      }),
    ];
    const inner = href
      ? [
          "a",
          {
            href,
            target: target === "_blank" ? "_blank" : null,
            rel: target === "_blank" ? "noopener noreferrer" : null,
          },
          image,
        ]
      : image;
    return [
      "figure",
      { class: `hd-image hd-image-${align || "center"}`, "data-align": align || "center" },
      inner,
      caption ? ["figcaption", {}, caption] : ["figcaption", { class: "is-empty" }],
    ];
  },
});

const Callout = Node.create({
  name: "callout",
  group: "block",
  content: "block+",
  defining: true,
  addAttributes() {
    return {
      variant: { default: "info" },
      title: { default: null },
    };
  },
  parseHTML() {
    return [{ tag: "aside[data-callout]" }];
  },
  renderHTML({ HTMLAttributes }) {
    const variant = HTMLAttributes.variant || "info";
    return [
      "aside",
      mergeAttributes(HTMLAttributes, { "data-callout": variant, class: `hd-callout hd-callout-${variant}` }),
      0,
    ];
  },
});

const Spacer = Node.create({
  name: "spacer",
  group: "block",
  atom: true,
  selectable: true,
  addAttributes() {
    return {
      height: { default: 32 },
    };
  },
  parseHTML() {
    return [{ tag: "div[data-spacer]" }];
  },
  renderHTML({ HTMLAttributes }) {
    const height = Number(HTMLAttributes.height) || 32;
    return ["div", mergeAttributes(HTMLAttributes, { "data-spacer": "", class: "hd-spacer", style: `height:${height}px` })];
  },
});

const Gallery = Node.create({
  name: "gallery",
  group: "block",
  atom: true,
  selectable: true,
  addAttributes() {
    return {
      items: { default: [] as Array<{ src: string; alt?: string }> },
    };
  },
  parseHTML() {
    return [{ tag: "div[data-gallery]" }];
  },
  renderHTML({ HTMLAttributes }) {
    const items = (HTMLAttributes.items ?? []) as Array<{ src: string; alt?: string }>;
    return [
      "div",
      { "data-gallery": "", class: "hd-gallery" },
      ...items.map((item) => ["figure", { class: "hd-gallery-item" }, ["img", { src: item.src, alt: item.alt ?? "", loading: "lazy" }]]),
    ];
  },
});

const FileDownload = Node.create({
  name: "fileDownload",
  group: "block",
  atom: true,
  selectable: true,
  addAttributes() {
    return {
      href: { default: null },
      name: { default: "دانلود فایل" },
    };
  },
  parseHTML() {
    return [{ tag: "p[data-file-download]" }];
  },
  renderHTML({ HTMLAttributes }) {
    return [
      "p",
      { "data-file-download": "", class: "hd-file" },
      ["a", { href: HTMLAttributes.href, download: "" }, HTMLAttributes.name || "دانلود فایل"],
    ];
  },
});

const Cta = Node.create({
  name: "cta",
  group: "block",
  atom: true,
  selectable: true,
  addAttributes() {
    return {
      href: { default: null },
      label: { default: "ادامه مطلب" },
    };
  },
  parseHTML() {
    return [{ tag: "p[data-cta]" }];
  },
  renderHTML({ HTMLAttributes }) {
    return [
      "p",
      { "data-cta": "", class: "hd-cta" },
      ["a", { class: "hd-cta-button", href: HTMLAttributes.href }, HTMLAttributes.label || "ادامه مطلب"],
    ];
  },
});

const ArticleLink = Node.create({
  name: "articleLink",
  group: "block",
  atom: true,
  selectable: true,
  addAttributes() {
    return {
      href: { default: null },
      title: { default: "مقاله مرتبط" },
      slug: { default: null },
    };
  },
  parseHTML() {
    return [{ tag: "p[data-article-link]" }];
  },
  renderHTML({ HTMLAttributes }) {
    return [
      "p",
      { "data-article-link": "", class: "hd-article-link" },
      ["a", { href: HTMLAttributes.href }, HTMLAttributes.title || "مقاله مرتبط"],
    ];
  },
});

const Terminal = Node.create({
  name: "terminal",
  group: "block",
  content: "text*",
  code: true,
  marks: "",
  defining: true,
  parseHTML() {
    return [{ tag: "pre.hd-terminal" }];
  },
  renderHTML({ HTMLAttributes }) {
    return ["pre", mergeAttributes(HTMLAttributes, { class: "hd-terminal" }), ["code", 0]];
  },
});

const HighlightedCodeBlock = CodeBlockLowlight.extend({
  addAttributes() {
    return {
      ...this.parent?.(),
      showLineNumbers: { default: true },
    };
  },
}).configure({ lowlight });

export function createArticleExtensions(placeholder = ARTICLE_EDITOR_PLACEHOLDER) {
  return [
    StarterKit.configure({
      heading: { levels: [2, 3, 4] },
      codeBlock: false,
      link: {
        openOnClick: false,
        autolink: true,
        HTMLAttributes: { rel: "noopener noreferrer" },
      },
    }),
    Underline,
    Highlight.configure({ multicolor: false }),
    TextStyle,
    Color,
    Typography,
    CharacterCount,
    Placeholder.configure({ placeholder }),
    TextAlign.configure({ types: ["heading", "paragraph"] }),
    ArticleImage.configure({ inline: false, allowBase64: false }),
    TaskList,
    TaskItem.configure({ nested: true }),
    Table.configure({ resizable: true }),
    TableRow,
    TableHeader,
    TableCell,
    Youtube.configure({ nocookie: true, width: 640, height: 360 }),
    HighlightedCodeBlock,
    Callout,
    Spacer,
    Gallery,
    FileDownload,
    Cta,
    ArticleLink,
    Terminal,
  ];
}

export function runSlashCommand(editor: Editor, command: string, extras?: Record<string, string>): boolean {
  const chain = editor.chain().focus();
  switch (command) {
    case "paragraph":
      return chain.setParagraph().run();
    case "heading2":
      return chain.setHeading({ level: 2 }).run();
    case "heading3":
      return chain.setHeading({ level: 3 }).run();
    case "heading4":
      return chain.setHeading({ level: 4 }).run();
    case "bulletList":
      return chain.toggleBulletList().run();
    case "orderedList":
      return chain.toggleOrderedList().run();
    case "taskList":
      return chain.toggleTaskList().run();
    case "blockquote":
      return chain.toggleBlockquote().run();
    case "horizontalRule":
      return chain.setHorizontalRule().run();
    case "codeBlock":
      return chain.setCodeBlock().run();
    case "table":
      return chain.insertTable({ rows: 3, cols: 3, withHeaderRow: true }).run();
    case "callout-info":
    case "callout-warning":
    case "callout-success":
    case "callout-note":
    case "callout-tip":
      return chain
        .insertContent({
          type: "callout",
          attrs: { variant: command.replace("callout-", ""), title: null },
          content: [{ type: "paragraph" }],
        })
        .run();
    case "spacer":
      return chain.insertContent({ type: "spacer", attrs: { height: 32 } }).run();
    case "terminal":
      return chain.insertContent({ type: "terminal", content: [{ type: "text", text: "$ " }] }).run();
    case "gallery":
      return chain.insertContent({ type: "gallery", attrs: { items: extras?.src ? [{ src: extras.src, alt: extras.alt ?? "" }] : [] } }).run();
    case "youtube":
      return chain.insertContent({ type: "youtube", attrs: { src: extras?.src ?? "" } }).run();
    case "fileDownload":
      return chain
        .insertContent({
          type: "fileDownload",
          attrs: { href: extras?.href ?? "/", name: extras?.name ?? "دانلود فایل" },
        })
        .run();
    case "cta":
      return chain
        .insertContent({
          type: "cta",
          attrs: { href: extras?.href ?? "/", label: extras?.label ?? "ادامه مطلب" },
        })
        .run();
    case "articleLink":
      return chain
        .insertContent({
          type: "articleLink",
          attrs: { href: extras?.href ?? "/articles/", title: extras?.title ?? "مقاله مرتبط", slug: extras?.slug ?? "" },
        })
        .run();
    case "image":
      if (!extras?.src) return false;
      return chain
        .insertContent({
          type: "image",
          attrs: {
            src: extras.src,
            mediaId: extras.mediaId ?? null,
            alt: extras.alt ?? "",
            title: extras.title ?? "",
            caption: extras.caption ?? "",
            align: extras.align ?? "center",
            width: extras.width ?? null,
            height: extras.height ?? null,
          },
        })
        .run();
    default:
      return false;
  }
}
