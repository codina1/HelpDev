"use client";

import Link from "next/link";
import { useEffect, useState } from "react";
import { motion } from "framer-motion";
import type { PromptDetailViewModel } from "@/data/prompt-detail";
import { formatCompactCount, formatPromptDate } from "@/data/prompt-detail";
import { PUBLIC_PROMPT_LAB_PATH } from "@/lib/public/prompt-lab-routes";

type PromptInfoSidebarProps = {
  model: PromptDetailViewModel;
};

function ShareButtons({
  title,
  onCopyLink,
  linkCopied,
}: {
  title: string;
  onCopyLink: () => void;
  linkCopied: boolean;
}) {
  const [pageUrl, setPageUrl] = useState("");
  useEffect(() => {
    setPageUrl(window.location.href);
  }, []);

  const encoded = encodeURIComponent(pageUrl);
  const text = encodeURIComponent(title);

  const links = [
    { label: "Telegram", href: pageUrl ? `https://t.me/share/url?url=${encoded}&text=${text}` : "#" },
    {
      label: "Twitter",
      href: pageUrl ? `https://twitter.com/intent/tweet?url=${encoded}&text=${text}` : "#",
    },
    {
      label: "LinkedIn",
      href: pageUrl ? `https://www.linkedin.com/sharing/share-offsite/?url=${encoded}` : "#",
    },
  ];

  return (
    <div className="mt-3 flex flex-wrap gap-2">
      {links.map((item) => (
        <a
          key={item.label}
          href={item.href}
          target={pageUrl ? "_blank" : undefined}
          rel={pageUrl ? "noopener noreferrer" : undefined}
          className="inline-flex h-9 items-center rounded-lg border border-white/[0.08] bg-[#070B18] px-2.5 text-[11px] font-bold text-[#94A3B8] transition hover:border-[#8B5CF6]/4 hover:text-white"
        >
          {item.label}
        </a>
      ))}
      <motion.button
        type="button"
        whileTap={{ scale: 0.97 }}
        onClick={onCopyLink}
        className="inline-flex h-9 items-center rounded-lg border border-[#8B5CF6]/35 bg-[#8B5CF6]/15 px-2.5 text-[11px] font-bold text-[#E9D5FF]"
      >
        {linkCopied ? "کپی شد" : "کپی لینک"}
      </motion.button>
    </div>
  );
}

export function PromptInfoSidebar({ model }: PromptInfoSidebarProps) {
  const { detail, levelLabel, rating, similar } = model;
  const [linkCopied, setLinkCopied] = useState(false);

  async function copyLink() {
    try {
      await navigator.clipboard.writeText(window.location.href);
      setLinkCopied(true);
      window.setTimeout(() => setLinkCopied(false), 1600);
    } catch {
      setLinkCopied(false);
    }
  }

  const infoRows = [
    { label: "دسته‌بندی", value: detail.category },
    { label: "مدل AI", value: detail.aiModel },
    { label: "سطح", value: levelLabel },
    { label: "آخرین بروزرسانی", value: formatPromptDate(detail.publishedAt) },
    { label: "استفاده", value: formatCompactCount(detail.viewCount) },
    { label: "امتیاز", value: rating.toLocaleString("fa-IR") },
  ];

  return (
    <div className="space-y-4">
      <Link
        href={PUBLIC_PROMPT_LAB_PATH}
        className="inline-flex h-9 items-center gap-2 rounded-xl border border-white/[0.08] bg-[#0B1224] px-3 text-[12.5px] font-bold text-[#CBD5E1] no-underline transition hover:border-[#8B5CF6]/35 hover:text-white"
      >
        ← بازگشت به لیست
      </Link>

      <aside className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4 shadow-[0_0_28px_rgba(139,92,246,0.1)] backdrop-blur-xl">
        <h2 className="text-[13px] font-extrabold text-white">اطلاعات کلی</h2>
        <dl className="mt-3 space-y-2.5">
          {infoRows.map((row) => (
            <div key={row.label} className="flex items-center justify-between gap-3 text-[12.5px]">
              <dt className="text-[#64748B]">{row.label}</dt>
              <dd className="font-bold text-[#E5E7EB]">{row.value}</dd>
            </div>
          ))}
        </dl>
      </aside>

      <aside className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4">
        <h2 className="text-[13px] font-extrabold text-white">اشتراک‌گذاری</h2>
        <ShareButtons title={detail.title} onCopyLink={() => void copyLink()} linkCopied={linkCopied} />
      </aside>

      <aside className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4">
        <h2 className="text-[13px] font-extrabold text-white">پرامپت‌های مشابه</h2>
        {similar.length === 0 ? (
          <p className="mt-3 text-[12px] text-[#64748B]">مورد مشابهی یافت نشد.</p>
        ) : (
          <ul className="mt-3 space-y-2.5">
            {similar.map((item) => (
              <li key={item.id}>
                <Link
                  href={`/prompt-lab/${item.slug}`}
                  className="block rounded-xl border border-transparent p-2 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
                >
                  <span className="line-clamp-2 text-[12.5px] font-bold leading-5 text-[#E5E7EB]">
                    {item.title}
                  </span>
                  <span className="mt-1 flex items-center gap-2 text-[11px] text-[#64748B]">
                    <span className="rounded bg-[#8B5CF6]/15 px-1.5 py-0.5 font-semibold text-[#C4B5FD]">
                      {item.category}
                    </span>
                    <span>{formatCompactCount(item.viewCount)} بازدید</span>
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </aside>
    </div>
  );
}
