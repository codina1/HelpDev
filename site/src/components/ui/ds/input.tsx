type InputProps = {
  id?: string;
  value?: string;
  defaultValue?: string;
  onChange?: (value: string) => void;
  onFocus?: () => void;
  placeholder?: string;
  type?: "text" | "search" | "email" | "password";
  "aria-label"?: string;
  className?: string;
  disabled?: boolean;
};

export function Input({
  id,
  value,
  defaultValue,
  onChange,
  onFocus,
  placeholder,
  type = "text",
  "aria-label": ariaLabel,
  className = "",
  disabled = false,
}: InputProps) {
  return (
    <input
      id={id}
      type={type}
      value={value}
      defaultValue={defaultValue}
      disabled={disabled}
      placeholder={placeholder}
      aria-label={ariaLabel}
      onFocus={onFocus}
      onChange={(e) => onChange?.(e.target.value)}
      className={[
        "focus-ring h-11 w-full rounded-[var(--ds-radius-lg)] border border-[color:var(--ds-border-strong)] bg-[color:var(--ds-bg-elevated)] px-3 text-sm text-[color:var(--ds-fg)] outline-none transition placeholder:text-[color:var(--ds-muted)]",
        "focus:border-[color:color-mix(in_srgb,var(--ds-primary)_50%,transparent)] focus:shadow-[0_0_0_3px_color-mix(in_srgb,var(--ds-primary)_18%,transparent)]",
        "disabled:opacity-50",
        className,
      ].join(" ")}
    />
  );
}
