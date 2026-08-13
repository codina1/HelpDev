import type { Metadata } from "next";
import { SITE } from "@/lib/constants";
import { ContactSection } from "./contact-section";

export const metadata: Metadata = {
  title: "تماس",
  description: `ارتباط با ${SITE.name} — فرم اعتبارسنجی می‌شود؛ سرویس ارسال هنوز فعال نیست.`,
};

export default function ContactPage() {
  return <ContactSection />;
}
