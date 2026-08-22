export type SlashCommandItem = {
  id: string;
  title: string;
  keywords: string[];
  group: "text" | "media" | "layout" | "technical";
  command: string;
};

export const SLASH_COMMANDS: SlashCommandItem[] = [
  { id: "paragraph", title: "متن معمولی", keywords: ["p", "متن", "paragraph", "text"], group: "text", command: "paragraph" },
  { id: "h2", title: "تیتر H2", keywords: ["h2", "heading", "تیتر"], group: "text", command: "heading2" },
  { id: "h3", title: "تیتر H3", keywords: ["h3", "heading"], group: "text", command: "heading3" },
  { id: "h4", title: "تیتر H4", keywords: ["h4", "heading"], group: "text", command: "heading4" },
  { id: "bullet", title: "فهرست نشانه‌دار", keywords: ["ul", "list", "فهرست"], group: "text", command: "bulletList" },
  { id: "ordered", title: "فهرست شماره‌دار", keywords: ["ol", "شماره"], group: "text", command: "orderedList" },
  { id: "task", title: "چک‌لیست", keywords: ["todo", "task", "چک"], group: "text", command: "taskList" },
  { id: "quote", title: "نقل‌قول", keywords: ["quote", "blockquote", "نقل"], group: "text", command: "blockquote" },
  { id: "callout-tip", title: "نکته", keywords: ["tip", "نکته", "callout"], group: "layout", command: "callout-tip" },
  { id: "callout-warning", title: "هشدار", keywords: ["warning", "هشدار"], group: "layout", command: "callout-warning" },
  { id: "callout-info", title: "اطلاعات", keywords: ["info", "اطلاعات", "callout"], group: "layout", command: "callout-info" },
  { id: "callout-success", title: "موفقیت", keywords: ["success", "موفقیت"], group: "layout", command: "callout-success" },
  { id: "divider", title: "جداکننده", keywords: ["hr", "divider", "جداکننده"], group: "layout", command: "horizontalRule" },
  { id: "image", title: "تصویر", keywords: ["image", "img", "عکس"], group: "media", command: "image" },
  { id: "table", title: "جدول", keywords: ["table", "جدول"], group: "technical", command: "table" },
  { id: "code", title: "Code Block", keywords: ["code", "کد"], group: "technical", command: "codeBlock" },
  { id: "youtube", title: "ویدئو", keywords: ["youtube", "video", "ویدئو", "ویدیو"], group: "media", command: "youtube" },
  { id: "spacer", title: "فاصله", keywords: ["spacer"], group: "layout", command: "spacer" },
  { id: "gallery", title: "گالری", keywords: ["gallery"], group: "media", command: "gallery" },
  { id: "file", title: "دانلود فایل", keywords: ["file", "download"], group: "media", command: "fileDownload" },
  { id: "terminal", title: "ترمینال", keywords: ["terminal", "bash"], group: "technical", command: "terminal" },
  { id: "cta", title: "دکمه فراخوان", keywords: ["cta", "button"], group: "technical", command: "cta" },
  { id: "articleLink", title: "پیوند مقاله داخلی", keywords: ["article", "link"], group: "technical", command: "articleLink" },
];

export function filterSlashCommands(query: string): SlashCommandItem[] {
  const q = query.trim().toLowerCase();
  if (!q) return SLASH_COMMANDS;
  const original = query.trim();
  return SLASH_COMMANDS.filter(
    (item) =>
      item.title.toLowerCase().includes(q) ||
      item.title.includes(original) ||
      item.id.includes(q) ||
      item.keywords.some((keyword) => keyword.toLowerCase().includes(q) || keyword.includes(original)),
  );
}
