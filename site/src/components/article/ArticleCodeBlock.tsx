"use client";

import { useMemo, useState } from "react";
import { motion } from "framer-motion";

type ArticleCodeBlockProps = {
  code: string;
  language?: string;
};

export function ArticleCodeBlock({ code, language = "tsx" }: ArticleCodeBlockProps) {
  const [copied, setCopied] = useState(false);
  const lines = useMemo(() => code.replace(/\r\n/g, "\n").split("\n"), [code]);

  async function onCopy() {
    try {
      await navigator.clipboard.writeText(code);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  }

  return (
    <div className="my-5 overflow-hidden rounded-2xl border border-[rgba(139,92,246,0.22)] bg-[#070B18] shadow-[0_0_28px_rgba(2,6,23,0.45)]">
      <div className="flex items-center justify-between border-b border-white/[0.08] px-3 py-2">
        <span className="rounded-md border border-white/[0.08] bg-white/[0.04] px-2 py-0.5 text-[11px] font-bold uppercase text-[#94A3B8]">
          {language}
        </span>
        <motion.button
          type="button"
          whileTap={{ scale: 0.97 }}
          onClick={() => void onCopy()}
          className="inline-flex h-8 items-center rounded-lg border border-white/[0.1] bg-[#0B1224] px-2.5 text-[11.5px] font-bold text-[#E5E7EB] transition hover:border-[#8B5CF6]/4"
        >
          {copied ? "کپی شد" : "کپی"}
        </motion.button>
      </div>
      <pre dir="ltr" className="m-0 grid max-h-[420px] grid-cols-[auto_1fr] gap-x-3 overflow-auto p-3 font-mono text-[12.5px] leading-6 text-[#CBD5E1]">
        <code className="select-none text-end text-[#475569]">
          {lines.map((_, index) => (
            <span key={index} className="block">
              {index + 1}
            </span>
          ))}
        </code>
        <code className="whitespace-pre-wrap break-words">
          {lines.map((line, index) => (
            <span key={index} className="block">
              {line || " "}
            </span>
          ))}
        </code>
      </pre>
    </div>
  );
}
