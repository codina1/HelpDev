"use client";

import type { SaveState } from "@/components/admin/content/editor/save-status";
import { SaveStatusIndicator } from "@/components/admin/content/editor/save-status";
import styles from "./article-rich-text-editor.module.css";

type EditorStatusBarProps = {
  wordCount: number;
  characterCount: number;
  readingTime: number;
  saveState?: SaveState;
  lastSavedAt?: string | null;
  fullscreen: boolean;
  onFullscreen: () => void;
};

export function EditorStatusBar({
  wordCount,
  characterCount,
  readingTime,
  saveState,
  lastSavedAt,
  fullscreen,
  onFullscreen,
}: EditorStatusBarProps) {
  return (
    <div className={styles.status} role="status">
      <div className="flex flex-wrap items-center gap-3">
        <span>{wordCount.toLocaleString("fa-IR")} واژه</span>
        <span>{characterCount.toLocaleString("fa-IR")} نویسه</span>
        <span>حدود {readingTime.toLocaleString("fa-IR")} دقیقه مطالعه</span>
        {saveState ? <SaveStatusIndicator state={saveState} /> : null}
        {lastSavedAt ? <span>آخرین ذخیره {new Date(lastSavedAt).toLocaleTimeString("fa-IR")}</span> : null}
      </div>
      <button
        type="button"
        className={styles.toolBtn}
        aria-label={fullscreen ? "خروج از تمام‌صفحه" : "تمام‌صفحه"}
        title={fullscreen ? "خروج از تمام‌صفحه" : "تمام‌صفحه"}
        onClick={onFullscreen}
      >
        {fullscreen ? "خروج" : "تمام‌صفحه"}
      </button>
    </div>
  );
}
