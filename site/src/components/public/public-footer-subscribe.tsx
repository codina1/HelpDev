"use client";

import { useState } from "react";
import {
  NEWSLETTER_UNAVAILABLE,
  isNewsletterEmail,
} from "@/components/public/home/home-newsletter-form";

/** Compact footer subscribe — same honest no-API behavior as the homepage form. */
export function PublicFooterSubscribe() {
  const [email, setEmail] = useState("");
  const [message, setMessage] = useState<string | null>(null);
  const [invalid, setInvalid] = useState(false);

  return (
    <form
      className="pub-footer-form"
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
      <label className="sr-only" htmlFor="pub-footer-email">
        ایمیل خبرنامه
      </label>
      <input
        id="pub-footer-email"
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
        aria-describedby={message ? "pub-footer-subscribe-status" : undefined}
        className="pub-footer-input"
      />
      <button type="submit" className="pub-footer-submit">
        عضویت
      </button>
      {message ? (
        <p id="pub-footer-subscribe-status" className="pub-footer-form-status" role="status">
          {message}
        </p>
      ) : null}
    </form>
  );
}
