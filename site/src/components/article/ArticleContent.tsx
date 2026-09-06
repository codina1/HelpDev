"use client";

import { useState } from "react";
import { motion } from "framer-motion";
import { ArticleCodeBlock } from "@/components/article/ArticleCodeBlock";
import { ArticleHtmlBody } from "@/components/public/articles/article-html-body";
import type { TocHeading } from "@/lib/public/content-helpers";

type ArticleContentProps = {
  usesBlocks: boolean;
  contentHtml: string | null;
  contentBody: string;
  headings: TocHeading[];
  tags: string[];
  hub?: "news" | "articles";
};

export function ArticleContent({
  usesBlocks,
  contentHtml,
  contentBody,
  headings,
  tags,
  hub = "articles",
}: ArticleContentProps) {
  const [useful, setUseful] = useState<"yes" | "no" | null>(null);
  const [saved, setSaved] = useState(false);
  const usefulLabel = hub === "news" ? "آیا این خبر مفید بود؟" : "آیا این مقاله مفید بود؟";
  const saveLabel = hub === "news" ? "ذخیره خبر" : "ذخیره مقاله";

  return (
    <div className="space-y-5">
      <div className="rounded-2xl border border-[rgba(139,92,246,0.18)] bg-[#10182D]/85 p-5 shadow-[0_0_28px_rgba(2,6,23,0.35)] backdrop-blur-xl sm:p-7">
        {usesBlocks ? (
          <ArticleHtmlBody html={contentHtml ?? ""} />
        ) : (
          <ReadingBody body={contentBody} headings={headings} />
        )}
      </div>

      {tags.length > 0 ? (
        <div className="flex flex-wrap gap-2">
          {tags.map((tag) => (
            <span
              key={tag}
              className="rounded-full border border-[rgba(139,92,246,0.28)] bg-[#8B5CF6]/12 px-3 py-1.5 text-[12px] font-bold text-[#E9D5FF]"
            >
              {tag}
            </span>
          ))}
        </div>
      ) : null}

      <div className="flex flex-wrap items-center justify-between gap-3 rounded-2xl border border-white/[0.08] bg-[#0B1224]/9 px-4 py-3.5">
        <p className="text-[13.5px] font-bold text-white">{usefulLabel}</p>
        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => setUseful("yes")}
            aria-pressed={useful === "yes"}
            className={[
              "focus-ring inline-flex h-9 items-center gap-1.5 rounded-xl border px-3 text-[12px] font-bold transition",
              useful === "yes"
                ? "border-emerald-400/40 bg-emerald-500/15 text-emerald-200"
                : "border-white/[0.1] bg-[#070B18] text-[#94A3B8] hover:text-white",
            ].join(" ")}
          >
            👍 بله
          </button>
          <button
            type="button"
            onClick={() => setUseful("no")}
            aria-pressed={useful === "no"}
            className={[
              "focus-ring inline-flex h-9 items-center gap-1.5 rounded-xl border px-3 text-[12px] font-bold transition",
              useful === "no"
                ? "border-rose-400/40 bg-rose-500/15 text-rose-200"
                : "border-white/[0.1] bg-[#070B18] text-[#94A3B8] hover:text-white",
            ].join(" ")}
          >
            👎 خیر
          </button>
          <motion.button
            type="button"
            whileTap={{ scale: 0.97 }}
            onClick={() => setSaved((v) => !v)}
            aria-pressed={saved}
            className={[
              "focus-ring inline-flex h-9 items-center rounded-xl px-3.5 text-[12px] font-bold text-white",
              saved
                ? "bg-[#6D28D9] shadow-[0_0_16px_rgba(139,92,246,0.35)]"
                : "bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] shadow-[0_0_16px_rgba(139,92,246,0.35)]",
            ].join(" ")}
          >
            {saved ? "ذخیره شد" : saveLabel}
          </motion.button>
        </div>
      </div>
    </div>
  );
}

export function Callout({ children }: { children: React.ReactNode }) {
  return (
    <aside className="my-5 flex gap-3 rounded-2xl border border-[rgba(139,92,246,0.35)] border-e-[3px] border-e-[#A855F7] bg-[#8B5CF6]/10 p-4 shadow-[0_0_24px_rgba(139,92,246,0.18)]">
      <span className="text-[18px]" aria-hidden>
        💡
      </span>
      <div>
        <p className="text-[13px] font-extrabold text-[#E9D5FF]">نکته مهم</p>
        <p className="mt-1 text-[13.5px] leading-7 text-[#CBD5E1]">{children}</p>
      </div>
    </aside>
  );
}

function ReadingBody({
  body,
  headings,
}: {
  body: string;
  headings: TocHeading[];
}) {
  const blocks = body.split(/\r?\n/);
  let headingIndex = 0;
  let firstHeading = true;
  const nodes: React.ReactNode[] = [];
  let codeBuffer: string[] | null = null;
  let codeLang = "txt";

  function flushCode(key: string) {
    if (!codeBuffer) return;
    nodes.push(
      <ArticleCodeBlock key={key} code={codeBuffer.join("\n")} language={codeLang} />,
    );
    codeBuffer = null;
    codeLang = "txt";
  }

  blocks.forEach((line, index) => {
    const fence = /^```(\w+)?\s*$/.exec(line.trim());
    if (fence) {
      if (codeBuffer) {
        flushCode(`code-${index}`);
      } else {
        codeBuffer = [];
        codeLang = fence[1] || "txt";
      }
      return;
    }
    if (codeBuffer) {
      codeBuffer.push(line);
      return;
    }

    const headingMatch = /^(#{1,3})\s+(.+?)\s*$/.exec(line.trim());
    if (headingMatch) {
      const level = headingMatch[1].length;
      const text = headingMatch[2].replace(/\{#[^}]+\}\s*$/, "").trim();
      const id =
        level >= 2 && headingIndex < headings.length ? headings[headingIndex++].id : undefined;
      const accent = firstHeading ? "border-e-[3px] border-[#8B5CF6] pe-3" : "";
      firstHeading = false;
      if (level >= 3) {
        nodes.push(
          <h3
            key={index}
            id={id}
            className={`scroll-mt-28 pt-3 text-[18px] font-extrabold text-white sm:text-[20px] ${accent}`}
          >
            {text}
          </h3>,
        );
      } else {
        nodes.push(
          <h2
            key={index}
            id={id}
            className={`scroll-mt-28 pt-4 text-[22px] font-extrabold text-white sm:text-[26px] ${accent}`}
          >
            {text}
          </h2>,
        );
      }
      return;
    }

    if (/^>\s?/.test(line.trim()) || line.includes("نکته مهم")) {
      nodes.push(
        <Callout key={index}>{line.replace(/^>\s?/, "").replace(/^💡\s?/, "").replace(/^نکته مهم:\s*/, "")}</Callout>,
      );
      return;
    }

    if (!line.trim()) {
      nodes.push(<div key={index} className="h-2" aria-hidden />);
      return;
    }

    if (/^[-*]\s+/.test(line.trim())) {
      nodes.push(
        <p key={index} className="text-[16px] leading-[1.95] text-[#94A3B8] sm:text-[17px]">
          <span className="me-2 text-[#8B5CF6]" aria-hidden>
            •
          </span>
          {line.replace(/^[-*]\s+/, "")}
        </p>,
      );
      return;
    }

    nodes.push(
      <p key={index} className="text-[16px] leading-[1.95] text-[#94A3B8] sm:text-[17px] sm:leading-[2]">
        {line}
      </p>,
    );
  });

  flushCode("code-end");

  if (!body.trim()) {
    return (
      <p className="rounded-xl border border-dashed border-white/[0.12] px-4 py-8 text-center text-sm text-[#94A3B8]">
        بدنه این محتوا خالی است.
      </p>
    );
  }

  return <div className="space-y-3.5 text-[#E2E8F0]">{nodes}</div>;
}
