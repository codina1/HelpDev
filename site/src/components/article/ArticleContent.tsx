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
};

const DEMO_CODE = `export default function Page() {
  return (
    <h1>Hello React 19</h1>
  );
}`;

export function ArticleContent({
  usesBlocks,
  contentHtml,
  contentBody,
  headings,
  tags,
}: ArticleContentProps) {
  const [useful, setUseful] = useState<"yes" | "no" | null>(null);
  const [saved, setSaved] = useState(false);

  return (
    <div className="space-y-6">
      <div className="rounded-2xl border border-[rgba(139,92,246,0.18)] bg-[#10182D]/85 p-5 shadow-[0_0_28px_rgba(2,6,23,0.35)] backdrop-blur-xl sm:p-7">
        {usesBlocks ? (
          <ArticleHtmlBody html={contentHtml ?? ""} />
        ) : (
          <ReadingBody body={contentBody} headings={headings} />
        )}

        {!usesBlocks && !/```/.test(contentBody) ? (
          <>
            <Callout>
              React 19 تغییرات مهمی در Server Components و Performance ارائه کرده است.
            </Callout>
            <ArticleCodeBlock code={DEMO_CODE} language="tsx" />
          </>
        ) : null}
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
        <p className="text-[13.5px] font-bold text-white">آیا این مقاله مفید بود؟</p>
        <div className="flex flex-wrap items-center gap-2">
          <button
            type="button"
            onClick={() => setUseful("yes")}
            className={[
              "inline-flex h-9 items-center gap-1.5 rounded-xl border px-3 text-[12px] font-bold transition",
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
            className={[
              "inline-flex h-9 items-center gap-1.5 rounded-xl border px-3 text-[12px] font-bold transition",
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
            className={[
              "inline-flex h-9 items-center rounded-xl px-3.5 text-[12px] font-bold text-white",
              saved
                ? "bg-[#6D28D9] shadow-[0_0_16px_rgba(139,92,246,0.35)]"
                : "bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] shadow-[0_0_16px_rgba(139,92,246,0.35)]",
            ].join(" ")}
          >
            {saved ? "ذخیره شد" : "ذخیره مقاله"}
          </motion.button>
        </div>
      </div>
    </div>
  );
}

export function Callout({ children }: { children: React.ReactNode }) {
  return (
    <aside className="my-5 flex gap-3 rounded-2xl border border-[rgba(139,92,246,0.35)] bg-[#8B5CF6]/10 p-4 shadow-[0_0_24px_rgba(139,92,246,0.18)]">
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
      if (level >= 3) {
        nodes.push(
          <h3
            key={index}
            id={id}
            className="scroll-mt-28 border-e-[3px] border-[#8B5CF6] pe-3 pt-3 text-[20px] font-extrabold text-white sm:text-[22px]"
          >
            {text}
          </h3>,
        );
      } else {
        nodes.push(
          <h2
            key={index}
            id={id}
            className="scroll-mt-28 border-e-[3px] border-[#8B5CF6] pe-3 pt-4 text-[24px] font-extrabold text-white sm:text-[28px]"
          >
            {text}
          </h2>,
        );
      }
      return;
    }

    if (/^>\s?/.test(line.trim()) || line.includes("نکته مهم")) {
      nodes.push(
        <Callout key={index}>{line.replace(/^>\s?/, "").replace(/^💡\s?/, "")}</Callout>,
      );
      return;
    }

    if (!line.trim()) {
      nodes.push(<div key={index} className="h-2" aria-hidden />);
      return;
    }

    nodes.push(
      <p key={index} className="text-[16.5px] leading-8 text-[#94A3B8] sm:text-[17.5px] sm:leading-9">
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

  return <div className="space-y-4 text-[#E2E8F0]">{nodes}</div>;
}
