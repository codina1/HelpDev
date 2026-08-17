import Link from "next/link";
import type { PromptLabCardItem } from "@/lib/public/prompt-lab-mock";
import type { PromptLabDetail } from "@/lib/public/prompt-lab-detail-mock";
import { publicPromptLabDetailPath } from "@/lib/public/prompt-lab-routes";
import styles from "./prompt-lab-detail-sidebar.module.css";

type PromptLabDetailSidebarProps = {
  detail: PromptLabDetail;
  related: readonly PromptLabCardItem[];
};

export function PromptLabDetailSidebar({ detail, related }: PromptLabDetailSidebarProps) {
  return (
    <aside className={styles.aside} aria-label="اطلاعات پرامپت">
      <section className={styles.panel} aria-labelledby="prompt-lab-related-heading">
        <h2 id="prompt-lab-related-heading" className={styles.heading}>
          پرامپت‌های مرتبط
        </h2>
        {related.length === 0 ? (
          <p className={styles.empty}>پرامپت مرتبطی در این دسته نیست.</p>
        ) : (
          <ul className={styles.related}>
            {related.map((item) => (
              <li key={item.id}>
                <Link href={publicPromptLabDetailPath(item.slug)} className={styles.relatedLink}>
                  <img src={item.coverImage} alt="" className={styles.thumb} />
                  <span>
                    <span className={styles.relatedTitle}>{item.title}</span>
                    <span className={styles.relatedMeta}>{item.category}</span>
                  </span>
                </Link>
              </li>
            ))}
          </ul>
        )}
      </section>

      <section className={styles.panel} aria-labelledby="prompt-lab-side-category-heading">
        <h2 id="prompt-lab-side-category-heading" className={styles.heading}>
          دسته
        </h2>
        <p className={styles.value}>{detail.category}</p>
      </section>

      <section className={styles.panel} aria-labelledby="prompt-lab-side-model-heading">
        <h2 id="prompt-lab-side-model-heading" className={styles.heading}>
          مدل
        </h2>
        <p className={styles.value}>{detail.aiModel}</p>
      </section>

      <section className={styles.panel} aria-labelledby="prompt-lab-side-tags-heading">
        <h2 id="prompt-lab-side-tags-heading" className={styles.heading}>
          برچسب‌ها
        </h2>
        <ul className={styles.tags}>
          {detail.tags.map((tag) => (
            <li key={tag} className={styles.tag}>
              {tag}
            </li>
          ))}
        </ul>
      </section>
    </aside>
  );
}
