import { PublicContainer } from "@/components/ui/public/v2/public-container";
import { ContactForm } from "./contact-form";
import styles from "./contact-section.module.css";

/**
 * Contact page form section — validation UI only, no backend.
 */
export function ContactSection() {
  return (
    <section className={`contact-section ${styles.section}`} aria-labelledby="contact-form-heading">
      <PublicContainer size="narrow">
        <div className={styles.panel}>
          <h1 id="contact-form-heading" className={styles.title}>
            تماس
          </h1>
          <p className={styles.lead}>
            فرم را پر کنید تا اعتبارسنجی شود. سرویس ارسال پیام هنوز فعال نیست.
          </p>
          <ContactForm />
        </div>
      </PublicContainer>
    </section>
  );
}
