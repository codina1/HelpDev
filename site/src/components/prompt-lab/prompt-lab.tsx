"use client";

import { useState } from "react";
import { enhancePrompt } from "@/lib/prompt-enhancer";

const PLACEHOLDER = 'e.g. "login api c#"';

export function PromptLab() {
  const [input, setInput] = useState("");
  const [output, setOutput] = useState("");
  const [copied, setCopied] = useState(false);

  function generate() {
    setOutput(enhancePrompt(input));
    setCopied(false);
  }

  async function copyOutput() {
    if (!output) return;

    try {
      await navigator.clipboard.writeText(output);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 1600);
    } catch {
      setCopied(false);
    }
  }

  return (
    <div className="space-y-5">
      <section className="ui-panel p-5 sm:p-6">
        <label
          htmlFor="prompt-request"
          className="ui-heading mb-3 block"
        >
          درخواست شما
        </label>
        <textarea
          id="prompt-request"
          value={input}
          onChange={(event) => setInput(event.target.value)}
          placeholder={PLACEHOLDER}
          rows={4}
          className="ui-input resize-y px-3.5 py-3 text-sm"
        />
        <div className="mt-4 flex flex-wrap items-center gap-2.5">
          <button
            type="button"
            onClick={generate}
            disabled={!input.trim()}
            className="ui-btn ui-btn-primary px-4 py-2.5"
          >
            ساخت پرامپت
          </button>
          <button
            type="button"
            onClick={() => {
              setInput("login api c#");
              setOutput(enhancePrompt("login api c#"));
              setCopied(false);
            }}
            className="ui-btn ui-btn-secondary px-4 py-2.5"
          >
            نمونه آماده
          </button>
        </div>
      </section>

      <section className="ui-panel p-5 sm:p-6">
        <div className="mb-3 flex items-center justify-between gap-3">
          <h2 className="ui-heading">پرامپت بهبودیافته</h2>
          <button
            type="button"
            onClick={copyOutput}
            disabled={!output}
            className={[
              "ui-btn px-3 py-2",
              copied ? "ui-btn-active" : "ui-btn-secondary",
            ].join(" ")}
          >
            {copied ? "کپی شد" : "کپی"}
          </button>
        </div>
        <pre className="min-h-48 whitespace-pre-wrap rounded-xl border border-border/80 bg-black/30 p-4 text-sm leading-relaxed text-muted shadow-inner">
          {output || "پرامپت ساخته‌شده اینجا نمایش داده می‌شود."}
        </pre>
      </section>
    </div>
  );
}
