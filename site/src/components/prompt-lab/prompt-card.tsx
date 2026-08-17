"use client";

import { useEffect, useState } from "react";
import styles from "./prompt-card.module.css";

const NUMBER_FA = new Intl.NumberFormat("fa-IR");

export const PROMPT_CARD_MEDIA_TYPES = ["Text", "Image", "Audio", "Video"] as const;

export type PromptCardMediaType = (typeof PROMPT_CARD_MEDIA_TYPES)[number];

const MEDIA_LABELS: Record<string, string> = {
  text: "متن",
  image: "تصویر",
  audio: "صدا",
  video: "ویدئو",
};

export type PromptCardModel = {
  title: string;
  description?: string | null;
  category: string;
  aiModel: string;
  mediaType: PromptCardMediaType | string;
  coverImage?: string | null;
  viewCount?: number;
  copyText?: string | null;
};

export type PromptCardProps = {
  item?: PromptCardModel | null;
  loading?: boolean;
  bookmarked?: boolean;
  copied?: boolean;
  onCopy?: () => void;
  onBookmark?: () => void;
  className?: string;
};

export function labelPromptCardMediaType(mediaType: string): string {
  return MEDIA_LABELS[mediaType.trim().toLowerCase()] ?? mediaType;
}

/**
 * Reusable Prompt Lab card — glass surface, copy/bookmark actions, loading and empty-cover states.
 */
export function PromptCard({
  item,
  loading = false,
  bookmarked = false,
  copied = false,
  onCopy,
  onBookmark,
  className = "",
}: PromptCardProps) {
  const [imageFailed, setImageFailed] = useState(false);
  const [copiedLocal, setCopiedLocal] = useState(false);

  useEffect(() => {
    setImageFailed(false);
  }, [item?.coverImage]);

  if (loading) {
    return <PromptCardSkeleton className={className} />;
  }

  if (!item) {
    return null;
  }

  const coverSrc = item.coverImage?.trim() ?? "";
  const showImage = coverSrc.length > 0 && !imageFailed;
  const mediaLabel = labelPromptCardMediaType(item.mediaType);
  const isCopied = copied || copiedLocal;
  const viewCount = item.viewCount ?? 0;
  const state = showImage ? "ready" : "empty-image";

  async function handleCopy() {
    const text = item.copyText?.trim();
    if (text && typeof navigator !== "undefined" && navigator.clipboard) {
      try {
        await navigator.clipboard.writeText(text);
        setCopiedLocal(true);
        window.setTimeout(() => setCopiedLocal(false), 1600);
      } catch {
        setCopiedLocal(false);
      }
    }
    onCopy?.();
  }

  return (
    <article
      dir="rtl"
      className={[styles.card, className].filter(Boolean).join(" ")}
      data-prompt-card={state}
    >
      <span className={styles.glow} aria-hidden />
      <div className={styles.visual}>
        {showImage ? (
          <img
            src={coverSrc}
            alt=""
            className={styles.image}
            onError={() => setImageFailed(true)}
          />
        ) : (
          <div className={styles.emptyCover} data-empty-cover>
            <span className={styles.emptyOrb} aria-hidden>
              <MediaGlyph mediaType={item.mediaType} />
            </span>
            <span className={styles.emptyLabel}>بدون تصویر</span>
          </div>
        )}
        <span className={styles.shade} aria-hidden />
        <span className={`${styles.badge} ${styles.media} ${styles.mediaChip}`}>{mediaLabel}</span>
      </div>
      <div className={styles.body}>
        <h3 className={styles.title}>{item.title}</h3>
        {item.description ? <p className={styles.description}>{item.description}</p> : null}
        <div className={styles.badges}>
          <span className={`${styles.badge} ${styles.category}`}>{item.category}</span>
          <span className={`${styles.badge} ${styles.model}`}>{item.aiModel}</span>
          <span className={`${styles.badge} ${styles.media}`}>{mediaLabel}</span>
        </div>
        <div className={styles.footer}>
          <button
            type="button"
            className={`${styles.iconButton} ${isCopied ? styles.copied : ""}`}
            onClick={handleCopy}
            aria-label="کپی پرامپت"
          >
            <CopyIcon />
            {isCopied ? "کپی شد" : "کپی"}
          </button>
          <button
            type="button"
            className={`${styles.iconButton} ${bookmarked ? styles.bookmarked : ""}`}
            onClick={onBookmark}
            aria-pressed={bookmarked}
            aria-label={bookmarked ? "حذف نشان" : "افزودن نشان"}
          >
            <BookmarkIcon filled={bookmarked} />
          </button>
          <span className={styles.views}>
            <ViewIcon />
            {NUMBER_FA.format(viewCount)} بازدید
          </span>
        </div>
      </div>
    </article>
  );
}

export function PromptCardSkeleton({ className = "" }: { className?: string }) {
  return (
    <article
      dir="rtl"
      className={[styles.card, className].filter(Boolean).join(" ")}
      data-prompt-card="loading"
      aria-busy="true"
      aria-live="polite"
    >
      <span className={styles.visuallyHidden}>در حال بارگذاری پرامپت</span>
      <div className={styles.visual}>
        <div className={`${styles.skeletonLine} ${styles.skeletonCover}`} />
      </div>
      <div className={styles.body}>
        <div className={`${styles.skeletonLine} ${styles.skeletonTitle}`} />
        <div className={`${styles.skeletonLine} ${styles.skeletonText}`} />
        <div className={`${styles.skeletonLine} ${styles.skeletonText} ${styles.skeletonTextShort}`} />
        <div className={styles.badges}>
          <span className={styles.skeletonBadge} />
          <span className={styles.skeletonBadge} />
          <span className={styles.skeletonBadge} />
        </div>
        <div className={styles.footer}>
          <span className={styles.skeletonButton} />
          <span className={styles.skeletonButton} />
          <span className={`${styles.skeletonButton} ${styles.views}`} />
        </div>
      </div>
    </article>
  );
}

function MediaGlyph({ mediaType }: { mediaType: string }) {
  const kind = mediaType.trim().toLowerCase();
  const common = {
    width: 16,
    height: 16,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.8,
  } as const;

  if (kind === "video") {
    return (
      <svg {...common}>
        <rect x="3" y="6" width="13" height="12" rx="2" />
        <path d="M16 10l5-3v10l-5-3z" />
      </svg>
    );
  }
  if (kind === "image") {
    return (
      <svg {...common}>
        <rect x="3" y="5" width="18" height="14" rx="2" />
        <circle cx="8.5" cy="10" r="1.4" />
        <path d="M21 16l-5.5-5.5L7 19" />
      </svg>
    );
  }
  if (kind === "audio") {
    return (
      <svg {...common}>
        <path d="M4 10v4h4l5 4V6L8 10H4zM16 9a3 3 0 0 1 0 6" />
      </svg>
    );
  }
  return (
    <svg {...common}>
      <path d="M7 7h10M7 12h10M7 17h6" />
    </svg>
  );
}

function CopyIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <rect x="8" y="8" width="12" height="12" rx="2" />
      <path d="M4 16V6a2 2 0 0 1 2-2h10" />
    </svg>
  );
}

function BookmarkIcon({ filled }: { filled: boolean }) {
  return (
    <svg
      width="13"
      height="13"
      viewBox="0 0 24 24"
      fill={filled ? "currentColor" : "none"}
      stroke="currentColor"
      strokeWidth="1.8"
      aria-hidden
    >
      <path d="M7 4h10a1 1 0 0 1 1 1v16l-6-3.4L6 21V5a1 1 0 0 1 1-1z" />
    </svg>
  );
}

function ViewIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M2 12s3.5-7 10-7 10 7 10 7-3.5 7-10 7S2 12 2 12z" />
      <circle cx="12" cy="12" r="2.5" />
    </svg>
  );
}
