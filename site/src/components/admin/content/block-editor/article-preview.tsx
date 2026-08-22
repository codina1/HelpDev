"use client";

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
        <div className="hd-article-html space-y-3 text-[15px] leading-8" dangerouslySetInnerHTML={{ __html: html }} />
      ) : (
        <p className="adm-subtle text-center text-[13px]">پیش‌نمایش خالی است.</p>
      )}
    </div>
  );
}
