"use client";

import { useState } from "react";
import styles from "./contact-form.module.css";

export const CONTACT_SUBJECTS = [
  "پیشنهاد همکاری",
  "گزارش مشکل",
  "پیشنهاد مقاله",
  "همکاری تجاری",
] as const;

export type ContactSubject = (typeof CONTACT_SUBJECTS)[number];

export const CONTACT_UNAVAILABLE = "سرویس تماس هنوز فعال نیست؛ پیامی ارسال نشد.";

export type ContactFields = {
  name: string;
  email: string;
  subject: string;
  message: string;
};

export type ContactErrors = Partial<Record<keyof ContactFields, string>>;

const EMAIL_RE = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;

export function isContactEmail(value: string): boolean {
  return EMAIL_RE.test(value.trim());
}

export function validateContactForm(fields: ContactFields): ContactErrors {
  const errors: ContactErrors = {};
  if (fields.name.trim().length < 2) {
    errors.name = "نام را وارد کنید.";
  }
  if (!isContactEmail(fields.email)) {
    errors.email = "یک ایمیل معتبر وارد کنید.";
  }
  if (!CONTACT_SUBJECTS.includes(fields.subject as ContactSubject)) {
    errors.subject = "یک موضوع انتخاب کنید.";
  }
  if (fields.message.trim().length < 10) {
    errors.message = "پیام را کامل‌تر بنویسید.";
  }
  return errors;
}

const EMPTY: ContactFields = { name: "", email: "", subject: "", message: "" };

/**
 * Contact form — client validation only. Does not call an API.
 */
export function ContactForm() {
  const [fields, setFields] = useState<ContactFields>(EMPTY);
  const [errors, setErrors] = useState<ContactErrors>({});
  const [status, setStatus] = useState<string | null>(null);

  function update<K extends keyof ContactFields>(key: K, value: ContactFields[K]) {
    setFields((current) => ({ ...current, [key]: value }));
    setErrors((current) => ({ ...current, [key]: undefined }));
    setStatus(null);
  }

  return (
    <form
      className={`contact-form ${styles.form}`}
      noValidate
      onSubmit={(event) => {
        event.preventDefault();
        const next = validateContactForm(fields);
        setErrors(next);
        if (Object.keys(next).length > 0) {
          setStatus(null);
          return;
        }
        setStatus(CONTACT_UNAVAILABLE);
      }}
    >
      <div className={styles.row}>
        <Field label="نام" error={errors.name} htmlFor="contact-name">
          <input
            id="contact-name"
            name="name"
            autoComplete="name"
            value={fields.name}
            onChange={(event) => update("name", event.target.value)}
            aria-invalid={Boolean(errors.name)}
            className={styles.input}
          />
        </Field>
        <Field label="ایمیل" error={errors.email} htmlFor="contact-email">
          <input
            id="contact-email"
            type="email"
            name="email"
            autoComplete="email"
            dir="ltr"
            value={fields.email}
            onChange={(event) => update("email", event.target.value)}
            aria-invalid={Boolean(errors.email)}
            className={styles.input}
            placeholder="you@company.com"
          />
        </Field>
      </div>

      <Field label="موضوع" error={errors.subject} htmlFor="contact-subject">
        <select
          id="contact-subject"
          name="subject"
          value={fields.subject}
          onChange={(event) => update("subject", event.target.value)}
          aria-invalid={Boolean(errors.subject)}
          className={styles.select}
        >
          <option value="">موضوع را انتخاب کنید</option>
          {CONTACT_SUBJECTS.map((subject) => (
            <option key={subject} value={subject}>
              {subject}
            </option>
          ))}
        </select>
      </Field>

      <Field label="پیام" error={errors.message} htmlFor="contact-message">
        <textarea
          id="contact-message"
          name="message"
          rows={6}
          value={fields.message}
          onChange={(event) => update("message", event.target.value)}
          aria-invalid={Boolean(errors.message)}
          className={styles.textarea}
        />
      </Field>

      <button type="submit" className={styles.submit}>
        ارسال پیام
      </button>
      {status ? (
        <p className={styles.status} role="status">
          {status}
        </p>
      ) : null}
    </form>
  );
}

function Field({
  label,
  htmlFor,
  error,
  children,
}: {
  label: string;
  htmlFor: string;
  error?: string;
  children: React.ReactNode;
}) {
  const errorId = `${htmlFor}-error`;
  return (
    <div className={styles.field}>
      <label className={styles.label} htmlFor={htmlFor}>
        {label}
      </label>
      {children}
      {error ? (
        <p id={errorId} className={styles.error} role="alert">
          {error}
        </p>
      ) : null}
    </div>
  );
}
