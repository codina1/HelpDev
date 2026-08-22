"use client";

import { useCallback, type MouseEvent } from "react";
import { sanitizeArticleHtml } from "@/lib/public/content-helpers";
import styles from "./article-html-body.module.css";

type ArticleHtmlBodyProps = {
  html: string;
};

export function ArticleHtmlBody({ html }: ArticleHtmlBodyProps) {
  const safe = sanitizeArticleHtml(html);

  const onClick = useCallback(async (event: MouseEvent<HTMLDivElement>) => {
    const pre = (event.target as HTMLElement).closest("pre");
    if (!pre) return;
    const code = pre.querySelector("code");
    const text = (code?.textContent ?? pre.textContent ?? "").trim();
    if (!text) return;
    const button = (event.target as HTMLElement).closest("button[data-copy]");
    if (!button) return;
    try {
      await navigator.clipboard.writeText(text);
      button.textContent = "کپی شد";
    } catch {
      button.textContent = "کپی نشد";
    }
  }, []);

  const decorated = safe.replace(
    /<pre(\b[^>]*)>/gi,
    '<div class="hd-code-wrap"><button type="button" data-copy class="hd-copy">کپی</button><pre$1>',
  ).replace(/<\/pre>/gi, "</pre></div>");

  return (
    <div
      className={styles.body}
      dir="rtl"
      onClick={(event) => void onClick(event)}
      dangerouslySetInnerHTML={{ __html: decorated }}
    />
  );
}
