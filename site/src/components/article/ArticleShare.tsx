"use client";

import { useEffect, useState } from "react";
import { motion } from "framer-motion";

type ArticleShareProps = {
  title: string;
};

export function ArticleShare({ title }: ArticleShareProps) {
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
  const links = [
    { label: "Telegram", href: pageUrl ? `https://t.me/share/url?url=${encoded}&text=${text}` : "#" },
    { label: "Twitter", href: pageUrl ? `https://twitter.com/intent/tweet?url=${encoded}&text=${text}` : "#" },
    {
      label: "LinkedIn",
      href: pageUrl ? `https://www.linkedin.com/sharing/share-offsite/?url=${encoded}` : "#",
    },
  ];

  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">اشتراک‌گذاری</h2>
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
          onClick={() => void copyLink()}
          className="inline-flex h-9 items-center rounded-lg border border-[#8B5CF6]/35 bg-[#8B5CF6]/15 px-2.5 text-[11px] font-bold text-[#E9D5FF]"
        >
          {copied ? "کپی شد" : "کپی لینک"}
        </motion.button>
      </div>
    </aside>
  );
}
