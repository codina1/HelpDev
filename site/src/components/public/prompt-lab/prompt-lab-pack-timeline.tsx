"use client";

import { useState } from "react";
import Link from "next/link";
import { Button } from "@/components/ui/ds/button";
import type { PromptLabPackListItem } from "@/lib/public/prompt-lab-pack-mock";
import { publicPromptLabDetailPath } from "@/lib/public/prompt-lab-routes";
import styles from "./prompt-lab-pack-timeline.module.css";

const INDEX_FA = new Intl.NumberFormat("fa-IR", { minimumIntegerDigits: 2 });

type PromptLabPackTimelineProps = {
  items: readonly PromptLabPackListItem[];
};

export function PromptLabPackTimeline({ items }: PromptLabPackTimelineProps) {
  const [copiedSlug, setCopiedSlug] = useState<string | null>(null);

  async function copyItem(item: PromptLabPackListItem) {
    try {
      await navigator.clipboard.writeText(item.content);
      setCopiedSlug(item.prompt.slug);
      window.setTimeout(() => setCopiedSlug(null), 2200);
    } catch {
      setCopiedSlug(null);
    }
  }

  return (
    <section className={styles.section} aria-labelledby="prompt-lab-pack-list-heading">
      <h2 id="prompt-lab-pack-list-heading" className={styles.heading}>
        فهرست پرامپت‌ها
      </h2>
      <ol className={styles.list}>
        {items.map((item) => (
          <li key={item.prompt.id} className={styles.item}>
            <span className={styles.index} aria-hidden>
              {INDEX_FA.format(item.order)}
            </span>
            <article className={styles.card}>
              <div className={styles.cardHead}>
                <h3 className={styles.title}>
                  <Link href={publicPromptLabDetailPath(item.prompt.slug)} className={styles.titleLink}>
                    {item.prompt.title}
                  </Link>
                </h3>
                <Button
                  type="button"
                  size="sm"
                  variant="secondary"
                  onClick={() => copyItem(item)}
                  aria-label={`کپی ${item.prompt.title}`}
                >
                  کپی
                </Button>
              </div>
              <p className={styles.description}>{item.prompt.description}</p>
              <pre className={styles.preview} dir="ltr">
                <code>{item.preview}</code>
              </pre>
            </article>
          </li>
        ))}
      </ol>
      {copiedSlug ? (
        <div className={styles.toast} role="status" aria-live="polite">
          پرامپت با موفقیت کپی شد
        </div>
      ) : null}
    </section>
  );
}
