"use client";

/** A small character counter; turns danger-colored when the limit is exceeded. */
export function CharacterCounter({
  value,
  max,
}: {
  value: number;
  max?: number;
}) {
  const over = typeof max === "number" && value > max;
  return (
    <span
      className={`text-[11px] tabular-nums ${over ? "text-[var(--adm-danger)]" : "adm-subtle"}`}
      aria-live="polite"
    >
      {typeof max === "number" ? `${value}/${max}` : `${value} نویسه`}
    </span>
  );
}
