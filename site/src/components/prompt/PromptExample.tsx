"use client";

import { motion } from "framer-motion";

type PromptExampleProps = {
  input: string;
  output: string;
};

export function PromptExample({ input, output }: PromptExampleProps) {
  return (
    <section id="example" className="mt-8 scroll-mt-28">
      <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">نمونه ورودی و خروجی</h2>

      <div
        dir="ltr"
        className="mt-4 grid grid-cols-1 items-stretch gap-3 lg:grid-cols-[1fr_auto_1fr] lg:gap-4"
      >
        <CodePanel title="Output" body={output} tone="output" />

        <div className="flex items-center justify-center py-2 lg:py-0">
          <motion.span
            aria-hidden
            animate={{ x: [0, -6, 0] }}
            transition={{ repeat: Infinity, duration: 1.6, ease: "easeInOut" }}
            className="inline-flex h-10 w-10 items-center justify-center rounded-full border border-[#8B5CF6]/4 bg-[#8B5CF6]/20 text-[#E9D5FF] shadow-[0_0_18px_rgba(139,92,246,0.4)]"
          >
            ←
          </motion.span>
        </div>

        <CodePanel title="Input" body={input} tone="input" />
      </div>
    </section>
  );
}

function CodePanel({
  title,
  body,
  tone,
}: {
  title: string;
  body: string;
  tone: "input" | "output";
}) {
  return (
    <div
      className={[
        "overflow-hidden rounded-2xl border bg-[#070B18]",
        tone === "output" ? "border-[#22D3EE]/25" : "border-white/[0.08]",
      ].join(" ")}
    >
      <div className="flex items-center justify-between border-b border-white/[0.08] px-3 py-2">
        <span className="text-[12px] font-bold text-white">{title}</span>
        <span className="text-[10px] font-bold text-[#64748B]">{tone === "output" ? "TSX" : "TXT"}</span>
      </div>
      <pre
        dir="ltr"
        className="m-0 max-h-[280px] overflow-auto p-3 font-mono text-[12px] leading-6 text-[#CBD5E1] whitespace-pre-wrap break-words"
      >
        {body}
      </pre>
    </div>
  );
}
