"use client";

import { parseMarkdown } from "@/lib/admin/content/markdown";

/** Document outline derived from the Markdown headings (levels 1–3). */
export function ContentOutline({ body }: { body: string }) {
  const headings = parseMarkdown(body).filter(
    (block): block is { kind: "heading"; level: 1 | 2 | 3; text: string } =>
      block.kind === "heading",
  );

  return (
    <nav aria-label="فهرست عناوین" className="space-y-2">
      <h3 className="adm-text text-[12px] font-bold">ساختار سند</h3>
      {headings.length === 0 ? (
        <p className="adm-subtle text-[11px]">هنوز عنوانی اضافه نشده است.</p>
      ) : (
        <ol className="space-y-1">
          {headings.map((heading, index) => (
            <li
              key={`${index}-${heading.text}`}
              className="adm-muted truncate text-[12px]"
              style={{ paddingInlineStart: `${(heading.level - 1) * 12}px` }}
            >
              {heading.text || "بدون عنوان"}
            </li>
          ))}
        </ol>
      )}
    </nav>
  );
}
