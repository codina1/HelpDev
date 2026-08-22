export type SlashCommandItem = {
  id: string;
  title: string;
  keywords: string[];
  group: "text" | "media" | "layout" | "technical";
  command: string;
};

export const SLASH_COMMANDS: SlashCommandItem[] = [
  { id: "paragraph", title: "پاراگراف", keywords: ["p", "متن", "paragraph"], group: "text", command: "paragraph" },
  { id: "h2", title: "عنوان ۲", keywords: ["h2", "heading"], group: "text", command: "heading2" },
  { id: "h3", title: "عنوان ۳", keywords: ["h3"], group: "text", command: "heading3" },
  { id: "h4", title: "عنوان ۴", keywords: ["h4"], group: "text", command: "heading4" },
  { id: "bullet", title: "فهرست نقطه‌ای", keywords: ["ul", "list"], group: "text", command: "bulletList" },
  { id: "ordered", title: "فهرست شماره‌ای", keywords: ["ol"], group: "text", command: "orderedList" },
  { id: "task", title: "چک‌لیست", keywords: ["todo", "task"], group: "text", command: "taskList" },
  { id: "quote", title: "نقل‌قول", keywords: ["quote", "blockquote"], group: "text", command: "blockquote" },
  { id: "callout-info", title: "نکته اطلاعاتی", keywords: ["info", "callout"], group: "layout", command: "callout-info" },
  { id: "callout-warning", title: "هشدار", keywords: ["warning"], group: "layout", command: "callout-warning" },
  { id: "callout-success", title: "موفقیت", keywords: ["success"], group: "layout", command: "callout-success" },
  { id: "callout-note", title: "یادداشت", keywords: ["note"], group: "layout", command: "callout-note" },
  { id: "callout-tip", title: "راهنمایی", keywords: ["tip"], group: "layout", command: "callout-tip" },
  { id: "divider", title: "جداکننده", keywords: ["hr", "divider"], group: "layout", command: "horizontalRule" },
  { id: "spacer", title: "فاصله", keywords: ["spacer"], group: "layout", command: "spacer" },
  { id: "image", title: "تصویر", keywords: ["image", "img", "عکس"], group: "media", command: "image" },
  { id: "gallery", title: "گالری", keywords: ["gallery"], group: "media", command: "gallery" },
  { id: "youtube", title: "ویدیو یوتیوب", keywords: ["youtube", "video", "embed"], group: "media", command: "youtube" },
  { id: "file", title: "دانلود فایل", keywords: ["file", "download"], group: "media", command: "fileDownload" },
  { id: "code", title: "کد", keywords: ["code"], group: "technical", command: "codeBlock" },
  { id: "terminal", title: "ترمینال", keywords: ["terminal", "bash"], group: "technical", command: "terminal" },
  { id: "table", title: "جدول", keywords: ["table"], group: "technical", command: "table" },
  { id: "cta", title: "دکمه فراخوان", keywords: ["cta", "button"], group: "technical", command: "cta" },
  { id: "articleLink", title: "پیوند مقاله داخلی", keywords: ["article", "link"], group: "technical", command: "articleLink" },
];

export function filterSlashCommands(query: string): SlashCommandItem[] {
  const q = query.trim().toLowerCase();
  if (!q) return SLASH_COMMANDS;
  return SLASH_COMMANDS.filter(
    (item) =>
      item.title.includes(query.trim()) ||
      item.id.includes(q) ||
      item.keywords.some((keyword) => keyword.includes(q)),
  );
}
