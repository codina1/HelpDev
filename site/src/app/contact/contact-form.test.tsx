import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import {
  CONTACT_SUBJECTS,
  CONTACT_UNAVAILABLE,
  ContactForm,
  isContactEmail,
  validateContactForm,
} from "./contact-form";
import { ContactSection } from "./contact-section";

describe("contact form", () => {
  it("renders name, email, subject options, and message", () => {
    const html = renderToStaticMarkup(<ContactSection />);
    expect(html).toContain("تماس");
    expect(html).toContain("نام");
    expect(html).toContain("ایمیل");
    expect(html).toContain("موضوع");
    expect(html).toContain("پیام");
    expect(html).toContain("ارسال پیام");
    for (const subject of CONTACT_SUBJECTS) {
      expect(html).toContain(subject);
    }
  });

  it("validates fields in the UI layer without a backend", () => {
    expect(isContactEmail("dev@helpdev.local")).toBe(true);
    expect(isContactEmail("bad")).toBe(false);
    const errors = validateContactForm({
      name: "",
      email: "bad",
      subject: "",
      message: "کوتاه",
    });
    expect(errors.name).toBeTruthy();
    expect(errors.email).toBeTruthy();
    expect(errors.subject).toBeTruthy();
    expect(errors.message).toBeTruthy();
    expect(
      validateContactForm({
        name: "محمد",
        email: "dev@helpdev.local",
        subject: "گزارش مشکل",
        message: "این یک پیام کامل برای تست اعتبارسنجی است.",
      }),
    ).toEqual({});
    expect(CONTACT_UNAVAILABLE).toContain("ارسال نشد");
    expect(renderToStaticMarkup(<ContactForm />)).toContain("contact-form");
  });
});
