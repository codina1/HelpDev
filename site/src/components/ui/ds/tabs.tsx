"use client";

type TabItem = {
  id: string;
  label: string;
};

type TabsProps = {
  items: TabItem[];
  value: string;
  onChange: (id: string) => void;
  "aria-label"?: string;
  className?: string;
};

export function Tabs({
  items,
  value,
  onChange,
  "aria-label": ariaLabel = "تب‌ها",
  className = "",
}: TabsProps) {
  return (
    <div
      role="tablist"
      aria-label={ariaLabel}
      className={["flex flex-wrap gap-1.5 rounded-[var(--ds-radius-lg)] border border-[color:var(--ds-border)] bg-[color:var(--ds-bg-elevated)] p-1", className].join(" ")}
    >
      {items.map((item) => {
        const selected = item.id === value;
        return (
          <button
            key={item.id}
            type="button"
            role="tab"
            aria-selected={selected}
            id={`tab-${item.id}`}
            className={[
              "focus-ring rounded-[var(--ds-radius-md)] px-3 py-1.5 text-[12px] font-semibold transition",
              selected
                ? "bg-[color:color-mix(in_srgb,var(--ds-primary)_20%,transparent)] text-[color:var(--ds-primary)]"
                : "text-[color:var(--ds-muted)] hover:bg-white/[0.04] hover:text-[color:var(--ds-fg)]",
            ].join(" ")}
            onClick={() => onChange(item.id)}
          >
            {item.label}
          </button>
        );
      })}
    </div>
  );
}
