"use client";

import type { ReactNode } from "react";

/** A single SEO input with an inline character counter, hint and error. */
export function SeoField({
  id,
  label,
  value,
  onChange,
  maxLength,
  hint,
  error,
  placeholder,
  disabled = false,
  multiline = false,
  ltr = false,
  action,
}: {
  id: string;
  label: string;
  value: string;
  onChange: (value: string) => void;
  maxLength: number;
  hint?: string;
  error?: string;
  placeholder?: string;
  disabled?: boolean;
  multiline?: boolean;
  ltr?: boolean;
  /** Optional inline action (e.g. a "browse media" button) shown next to the counter. */
  action?: ReactNode;
}) {
  const count = value.trim().length;
  const over = count > maxLength;

  return (
    <div className="space-y-1.5">
      <div className="flex items-center justify-between gap-2">
        <label htmlFor={id} className="adm-text text-[12px] font-semibold">
          {label}
        </label>
        <div className="flex items-center gap-2">
          {action}
          <span
            className={`text-[11px] tabular-nums ${
              over ? "text-[var(--adm-danger)]" : "adm-subtle"
            }`}
            aria-live="polite"
          >
            {count}/{maxLength}
          </span>
        </div>
      </div>

      {multiline ? (
        <textarea
          id={id}
          className="adm-input min-h-[80px] resize-y text-[13px] leading-6"
          value={value}
          disabled={disabled}
          dir={ltr ? "ltr" : undefined}
          placeholder={placeholder}
          onChange={(event) => onChange(event.target.value)}
          aria-invalid={Boolean(error) || over}
        />
      ) : (
        <input
          id={id}
          type="text"
          className={`adm-input ${ltr ? "text-start" : ""}`}
          value={value}
          disabled={disabled}
          dir={ltr ? "ltr" : undefined}
          placeholder={placeholder}
          onChange={(event) => onChange(event.target.value)}
          aria-invalid={Boolean(error) || over}
        />
      )}

      {hint && !error ? <p className="adm-subtle text-[11px]">{hint}</p> : null}
      {error ? (
        <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{error}</p>
      ) : null}
    </div>
  );
}
