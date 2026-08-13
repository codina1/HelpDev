import type { Metadata } from "next";
import { SITE } from "@/lib/constants";
import { ContactFaq } from "./contact-faq";
import { ContactInfo } from "./contact-info";
import { ContactSection } from "./contact-section";

export const metadata: Metadata = {
  title: "تماس",
  description: `ارتباط با ${SITE.name} — فرم اعتبارسنجی می‌شود؛ سرویس ارسال هنوز فعال نیست.`,
};

export default function ContactPage() {
  return (
    <>
      <ContactInfo />
      <ContactSection />
      <ContactFaq />
    </>
  );
}
