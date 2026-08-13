"use client";

import { useState } from "react";

export const NEWSLETTER_UNAVAILABLE =
  "سرویس خبرنامه هنوز فعال نیست؛ ایمیلی ذخیره نشد.";

export function isNewsletterEmail(value: string): boolean {
  const email = value.trim();
  return /^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email);
}

/** Email + CTA — does not invent a working subscribe API. */
export function HomeNewsletterForm() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [invalid, setInvalid] = useState(false);

  return (
    <form
      className="home-newsletter-form"
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        if (!isNewsletterEmail(email)) {
          setInvalid(true);
          setMessage("یک ایمیل معتبر وارد کنید.");
          return;
        }
        setInvalid(false);
        setMessage(NEWSLETTER_UNAVAILABLE);
      }}
    >
      <label className="sr-only" htmlFor="home-newsletter-email">
        ایمیل خبرنامه
      </label>
      <input
        id="home-newsletter-email"
        type="email"
        name="email"
        autoComplete="email"
        dir="ltr"
        value={email}
        onChange={(event) => {
          setEmail(event.target.value);
          setInvalid(false);
        }}
        placeholder="you@company.com"
        aria-invalid={invalid}
        aria-describedby={message ? "home-newsletter-status" : undefined}
        className="home-newsletter-input"
      />
      <button type="submit" className="home-newsletter-cta">
        عضویت
      </button>
      {message ? (
        <p id="home-newsletter-status" className="home-newsletter-status" role="status">
          {message}
        </p>
      ) : null}
    </form>
  );
}
