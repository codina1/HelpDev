"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { motion } from "framer-motion";
import type { ToolDetailModel, ToolRelatedTool } from "@/data/tool-detail";
import { publicToolPath } from "@/data/tool-detail";

export function ToolSidebar({ model }: { model: ToolDetailModel }) {
  return (
    <div className="space-y-4">
      <ToolInfoCard model={model} />
      <ToolRating model={model} />
      <ShareTools title={model.name} />
      <RelatedTools items={model.relatedTools} />
      <ToolCTA />
    </div>
  );
}

export function ToolInfoCard({ model }: { model: ToolDetailModel }) {
  const rows = [
    { label: "توسعه‌دهنده", value: model.developer },
    { label: "نوع ابزار", value: model.toolType },
    { label: "لایسنس", value: model.license },
    { label: "آخرین نسخه", value: model.version },
    { label: "تاریخ انتشار", value: model.releaseDateLabel },
    { label: "دانلود", value: model.downloadsLabel },
    { label: "سیستم‌عامل", value: model.platforms.join(" · ") },
  ];

  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4 shadow-[0_0_28px_rgba(139,92,246,0.1)] backdrop-blur-xl">
      <h2 className="text-[13px] font-extrabold text-white">اطلاعات کلی</h2>
      <dl className="mt-3 space-y-2.5">
        {rows.map((row) => (
          <div key={row.label} className="flex items-start justify-between gap-3 text-[12.5px]">
            <dt className="shrink-0 text-[#64748B]">{row.label}</dt>
            <dd className="text-end font-bold text-[#E5E7EB]">{row.value}</dd>
          </div>
        ))}
      </dl>
      <a
        href={model.websiteUrl}
        target="_blank"
        rel="noopener noreferrer"
        className="mt-3 inline-flex text-[12px] font-bold text-[#C4B5FD] hover:text-white"
      >
        وب‌سایت رسمی ↗
      </a>
    </aside>
  );
}

export function ToolRating({ model }: { model: ToolDetailModel }) {
  return (
    <aside className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">امتیاز کاربران</h2>
      <div className="mt-3 flex items-end gap-3">
        <p className="text-[32px] font-extrabold leading-none text-white">
          {model.rating.toLocaleString("fa-IR")}
        </p>
        <div>
          <p className="text-[13px] text-[#FBBF24]">★★★★★</p>
          <p className="text-[11px] text-[#64748B]">
            از ۵ · {model.ratingCount.toLocaleString("fa-IR")} رأی
          </p>
        </div>
      </div>
      <ul className="mt-4 space-y-1.5">
        {model.ratingBreakdown.map((row) => (
          <li key={row.stars} className="flex items-center gap-2 text-[11px] text-[#94A3B8]">
            <span className="w-6">{row.stars}★</span>
            <span className="h-1.5 flex-1 overflow-hidden rounded-full bg-white/[0.06]">
              <span
                className="block h-full rounded-full bg-[#8B5CF6]"
                style={{ width: `${row.percent}%` }}
              />
            </span>
            <span className="w-8 text-end">{row.percent.toLocaleString("fa-IR")}٪</span>
          </li>
        ))}
      </ul>
    </aside>
  );
}

export function ShareTools({ title }: { title: string }) {
  const [pageUrl, setPageUrl] = useState("");
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    setPageUrl(window.location.href);
  }, []);

  async function copyLink() {
    try {
      await navigator.clipboard.writeText(window.location.href);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  }

  const encoded = encodeURIComponent(pageUrl);
  const text = encodeURIComponent(title);

  return (
    <aside className="rounded-2xl border border-white/[0.08] bg-[#10182D]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">اشتراک‌گذاری</h2>
      <div className="mt-3 flex flex-wrap gap-2">
        {[
          { label: "Telegram", href: pageUrl ? `https://t.me/share/url?url=${encoded}&text=${text}` : "#" },
          { label: "Twitter", href: pageUrl ? `https://twitter.com/intent/tweet?url=${encoded}&text=${text}` : "#" },
          {
            label: "LinkedIn",
            href: pageUrl ? `https://www.linkedin.com/sharing/share-offsite/?url=${encoded}` : "#",
          },
        ].map((item) => (
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
          onClick={() => void copyLink()}
          className="inline-flex h-9 items-center rounded-lg border border-[#8B5CF6]/35 bg-[#8B5CF6]/15 px-2.5 text-[11px] font-bold text-[#E9D5FF]"
        >
          {copied ? "کپی شد" : "کپی لینک"}
        </motion.button>
      </div>
    </aside>
  );
}

export function RelatedTools({ items }: { items: ToolRelatedTool[] }) {
  if (items.length === 0) return null;
  return (
    <aside id="similar" className="scroll-mt-28 rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">ابزارهای مشابه</h2>
      <ul className="mt-3 space-y-2">
        {items.map((item) => (
          <li key={item.id}>
            <Link
              href={publicToolPath(item.slug)}
              className="group flex items-center gap-2.5 rounded-xl border border-transparent p-2 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
            >
              <span className="inline-flex h-10 w-10 items-center justify-center rounded-xl bg-[#8B5CF6]/15 text-[12px] font-bold text-[#E9D5FF]">
                {item.name.slice(0, 2)}
              </span>
              <span className="min-w-0 flex-1">
                <span className="block truncate text-[12.5px] font-bold text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                  {item.name}
                </span>
                <span className="text-[11px] text-[#64748B]">{item.categoryLabel}</span>
              </span>
              <span className="text-[#64748B]">←</span>
            </Link>
          </li>
        ))}
      </ul>
    </aside>
  );
}

export function ToolCTA() {
  return (
    <motion.aside
      whileHover={{ y: -2 }}
      className="rounded-2xl border border-[rgba(139,92,246,0.4)] bg-[linear-gradient(145deg,rgba(139,92,246,0.32),rgba(37,99,235,0.12)_45%,rgba(11,18,36,0.95))] p-4 shadow-[0_0_28px_rgba(139,92,246,0.22)]"
    >
      <h2 className="text-[14px] font-extrabold text-white">ابزار مناسب نیاز نیست؟</h2>
      <p className="mt-1.5 text-[12px] leading-6 text-[#CBD5E1]">
        مجموعه کامل ابزارهای HelpDev را بررسی کن.
      </p>
      <Link
        href="/toolbox"
        className="mt-3 inline-flex h-9 items-center rounded-xl bg-white/95 px-3.5 text-[12px] font-bold text-[#4C1D95] no-underline"
      >
        مشاهده همه ابزارها
      </Link>
    </motion.aside>
  );
}
