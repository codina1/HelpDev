"use client";

import { useState } from "react";
import Link from "next/link";
import { PublicContainer } from "@/components/ui/public/v2/public-container";
import styles from "./contact-faq.module.css";

export const CONTACT_FAQ_ITEMS = [
  {
    id: "article",
    question: "چطور مقاله ارسال کنم؟",
    answer: (
      <>
        از صفحه{" "}
        <Link href="/write" className={styles.link}>
          نویسنده شو
        </Link>{" "}
        وارد حساب شوید. اگر نقش نویسنده یا ادمین داشته باشید می‌توانید مطلب ثبت کنید.
        ارسال عمومی بدون حساب هنوز فعال نیست.
      </>
    ),
  },
  {
    id: "writers",
    question: "آیا همکاری با نویسندگان دارید؟",
    answer: (
      <>
        بله، مسیر نویسنده در{" "}
        <Link href="/write" className={styles.link}>
          نویسنده شو
        </Link>{" "}
        است و نقش نویسنده را ادمین می‌دهد. برای پیشنهاد همکاری یا مقاله، موضوع
        مربوط را در فرم همین صفحه انتخاب کنید. سرویس ارسال فرم هنوز فعال نیست.
      </>
    ),
  },
  {
    id: "tool",
    question: "چگونه ابزار خودم را معرفی کنم؟",
    answer: (
      <>
        فرم عمومی معرفی ابزار نداریم. موضوع «پیشنهاد همکاری» را در فرم تماس انتخاب
        کنید. تا فعال شدن سرویس ارسال، پیامی ذخیره نمی‌شود.
      </>
    ),
  },
] as const;

/**
 * Contact FAQ — accordion UI only, honest answers, no invented workflows.
 */
export function ContactFaq() {
  const [openId, setOpenId] = useState<string | null>(CONTACT_FAQ_ITEMS[0].id);

  return (
    <section className={`contact-faq ${styles.section}`} aria-labelledby="contact-faq-heading">
      <PublicContainer size="narrow">
        <h2 id="contact-faq-heading" className={styles.heading}>
          پرسش‌های پرتکرار
        </h2>
        <ul className={styles.list}>
          {CONTACT_FAQ_ITEMS.map((item) => {
            const open = openId === item.id;
            return (
              <li key={item.id} className={`${styles.item} ${open ? styles.itemOpen : ""}`}>
                <button
                  type="button"
                  className={styles.trigger}
                  aria-expanded={open}
                  aria-controls={`contact-faq-${item.id}`}
                  onClick={() => setOpenId(open ? null : item.id)}
                >
                  <span>{item.question}</span>
                  <span className={styles.chevron} aria-hidden>
                    <ChevronIcon />
                  </span>
                </button>
                <div className={styles.answer} id={`contact-faq-${item.id}`}>
                  <div className={styles.answerInner}>
                    <p className={styles.copy}>{item.answer}</p>
                  </div>
                </div>
              </li>
            );
          })}
        </ul>
      </PublicContainer>
    </section>
  );
}

function ChevronIcon() {
  return (
    <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="m6 9 6 6 6-6" />
    </svg>
  );
}
