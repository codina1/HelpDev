"use client";

import { ArticleHtmlBody } from "@/components/public/articles/article-html-body";
import styles from "./article-block-editor.module.css";

export type PreviewDevice = "desktop" | "tablet" | "mobile";

type ArticlePreviewProps = {
  html: string;
  device: PreviewDevice;
};

export function ArticlePreview({ html, device }: ArticlePreviewProps) {
  const deviceClass =
    device === "mobile" ? styles.previewMobile : device === "tablet" ? styles.previewTablet : styles.previewDesktop;

  return (
    <div className={`${styles.previewFrame} ${deviceClass}`} dir="rtl">
      {html.trim() ? (
        <ArticleHtmlBody html={html} />
      ) : (
        <p className="adm-subtle text-center text-[13px]">پیش‌نمایش خالی است.</p>
      )}
    </div>
  );
}
