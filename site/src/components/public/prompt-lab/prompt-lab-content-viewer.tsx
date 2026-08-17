"use client";

import { useState } from "react";
import { Button } from "@/components/ui/ds/button";
import styles from "./prompt-lab-content-viewer.module.css";

type PromptLabContentViewerProps = {
  content: string;
};

export function PromptLabContentViewer({ content }: PromptLabContentViewerProps) {
  const [copied, setCopied] = useState(false);
  const lines = content.replace(/\r\n/g, "\n").split("\n");

  async function copyContent() {
    try {
      await navigator.clipboard.writeText(content);
      setCopied(true);
      window.setTimeout(() => setCopied(false), 2200);
    } catch {
      setCopied(false);
    }
  }

  return (
    <section className={styles.section} aria-labelledby="prompt-lab-content-heading">
      <div className={styles.toolbar}>
        <h2 id="prompt-lab-content-heading" className={styles.heading}>
          متن پرامپت
        </h2>
        <Button type="button" size="sm" variant="secondary" onClick={copyContent} aria-label="کپی پرامپت">
          کپی
        </Button>
      </div>
      <div className={styles.frame}>
        <pre className={styles.pre} dir="ltr">
          <code className={styles.code}>
            {lines.map((line, index) => (
              <span key={`line-${index}`} className={styles.line}>
                <span className={styles.gutter} aria-hidden>
                  {index + 1}
                </span>
                <span className={styles.source}>{line.length > 0 ? line : " "}</span>
              </span>
            ))}
          </code>
        </pre>
      </div>
      {copied ? (
        <div className={styles.toast} role="status" aria-live="polite">
          پرامپت با موفقیت کپی شد
        </div>
      ) : null}
    </section>
  );
}
