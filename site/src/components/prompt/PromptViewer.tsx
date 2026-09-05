"use client";

import { useMemo, useState } from "react";
import { motion } from "framer-motion";

type PromptViewerProps = {
  content: string;
  language: string;
};

export function PromptViewer({ content, language }: PromptViewerProps) {
  const [copied, setCopied] = useState(false);
  const lines = useMemo(() => content.replace(/\r\n/g, "\n").split("\n"), [content]);

  async function onCopy() {
    try {
      await navigator.clipboard.writeText(content);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  }

  return (
    <section id="prompt" className="scroll-mt-28">
      <div className="flex items-center justify-between gap-3">
        <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">متن کامل پرامپت</h2>
      </div>

      <div className="mt-3 overflow-hidden rounded-2xl border border-white/[0.08] bg-[#070B18] shadow-[0_0_28px_rgba(2,6,23,0.45)]">
        <div className="flex items-center justify-between border-b border-white/[0.08] px-3 py-2">
          <span className="rounded-md border border-white/[0.08] bg-white/[0.04] px-2 py-0.5 text-[11px] font-bold text-[#94A3B8]">
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

        <div className="max-h-[420px] overflow-auto" dir="ltr">
          <pre className="m-0 grid grid-cols-[auto_1fr] gap-x-3 p-3 font-mono text-[12.5px] leading-6 text-[#E2E8F0] sm:text-[13px]">
            <code className="select-none text-end text-[#475569]">
              {lines.map((_, index) => (
                <span key={index} className="block">
                  {index + 1}
                </span>
              ))}
            </code>
            <code className="whitespace-pre-wrap break-words text-[#CBD5E1]">
              {lines.map((line, index) => (
                <span key={index} className="block">
                  {line || " "}
                </span>
              ))}
            </code>
          </pre>
        </div>
      </div>
    </section>
  );
}
