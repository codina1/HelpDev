"use client";

type SearchBoxProps = {
  value: string;
  onChange: (value: string) => void;
  onSubmit?: (value: string) => void;
  onFocus?: () => void;
  placeholder?: string;
  /** Shown as trailing kbd hint (e.g. Ctrl K) */
  shortcutHint?: string;
  className?: string;
  inputClassName?: string;
  id?: string;
  "aria-label"?: string;
  autoFocus?: boolean;
  size?: "md" | "lg";
};

/**
 * Accessible public search field. Does not fetch — parent owns search behavior.
 */
export function SearchBox({
  value,
  onChange,
  onSubmit,
  onFocus,
  placeholder = "جستجو...",
  shortcutHint,
  className = "",
  inputClassName = "",
  id,
  "aria-label": ariaLabel = "جستجو",
  autoFocus = false,
  size = "md",
}: SearchBoxProps) {
  return (
    <form
      className={["relative w-full", className].join(" ")}
      role="search"
      onSubmit={(event) => {
        event.preventDefault();
        onSubmit?.(value.trim());
      }}
    >
      <span
        className="pointer-events-none absolute inset-y-0 start-3.5 flex items-center text-[color:var(--muted)]"
        aria-hidden
      >
        <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2">
          <circle cx="11" cy="11" r="7" />
          <path d="m20 20-3-3" />
        </svg>
      </span>
      <input
        id={id}
        type="search"
        value={value}
        autoFocus={autoFocus}
        onChange={(e) => onChange(e.target.value)}
        onFocus={onFocus}
        placeholder={placeholder}
        aria-label={ariaLabel}
        className={[
          "focus-ring w-full rounded-xl border border-[color:var(--border-strong)] bg-[color:var(--surface-elevated)] text-[color:var(--foreground)] outline-none transition placeholder:text-[color:var(--muted)]",
          "focus:border-[color:color-mix(in_srgb,var(--accent)_50%,transparent)] focus:shadow-[0_0_0_3px_var(--accent-soft)]",
          size === "lg" ? "h-14 pe-20 ps-11 text-base" : "h-11 pe-16 ps-10 text-sm",
          inputClassName,
        ].join(" ")}
      />
      {shortcutHint ? (
        <kbd
          className={[
            "pointer-events-none absolute inset-y-0 end-2.5 my-auto hidden items-center rounded-md border border-[color:var(--border)] bg-[color:var(--surface)] px-1.5 font-medium text-[color:var(--muted)] sm:inline-flex",
            size === "lg" ? "h-7 text-[11px]" : "h-6 text-[10px]",
          ].join(" ")}
        >
          {shortcutHint}
        </kbd>
      ) : null}
    </form>
  );
}
