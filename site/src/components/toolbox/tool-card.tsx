"use client";

import { useState } from "react";
import type { ToolItem } from "@/types";

type ToolCardProps = {
  item: ToolItem;
};

export function ToolCard({ item }: ToolCardProps) {
  const [copied, setCopied] = useState(false);

  async function copyContent() {
    try {
      await navigator.clipboard.writeText(item.content);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  }

  return (
    <article className="ui-card flex h-full flex-col p-5">
      <div className="flex items-start justify-between gap-4">
        <div className="min-w-0">
          <h2 className="ui-heading">{item.title}</h2>
          <p className="ui-body mt-2">{item.description}</p>
        </div>
        <button
          type="button"
          onClick={copyContent}
          className={[
            "ui-btn shrink-0 px-3 py-2",
            copied ? "ui-btn-active" : "ui-btn-secondary",
          ].join(" ")}
          aria-label={`Copy ${item.title}`}
        >
          <span className="inline-flex items-center gap-1.5">
            {copied ? <CheckIcon /> : <CopyIcon />}
            {copied ? "کپی شد" : "کپی"}
          </span>
        </button>
      </div>

      <pre className="mt-5 max-h-36 overflow-hidden rounded-xl border border-border/80 bg-black/30 p-3.5 font-mono text-[11px] leading-relaxed text-muted shadow-inner">
        <code>{item.content}</code>
      </pre>
    </article>
  );
}

function CopyIcon() {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <rect x="9" y="9" width="13" height="13" rx="2" />
      <path d="M5 15H4a2 2 0 0 1-2-2V4a2 2 0 0 1 2-2h9a2 2 0 0 1 2 2v1" />
    </svg>
  );
}

function CheckIcon() {
  return (
    <svg
      width="14"
      height="14"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <path d="M20 6 9 17l-5-5" />
    </svg>
  );
}
