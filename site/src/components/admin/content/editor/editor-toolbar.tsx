"use client";

export type ToolbarCommand =
  | "bold"
  | "italic"
  | "code"
  | "link"
  | "heading"
  | "unordered"
  | "ordered";

const BUTTONS: Array<{ command: ToolbarCommand; label: string; title: string }> = [
  { command: "heading", label: "H", title: "عنوان" },
  { command: "bold", label: "B", title: "پررنگ" },
  { command: "italic", label: "I", title: "مورب" },
  { command: "code", label: "</>", title: "کد" },
  { command: "link", label: "🔗", title: "پیوند" },
  { command: "unordered", label: "•", title: "فهرست نقطه‌ای" },
  { command: "ordered", label: "1.", title: "فهرست شماره‌دار" },
];

/** Formatting toolbar for the Markdown editor. Purely dispatches commands. */
export function EditorToolbar({
  onCommand,
  disabled = false,
}: {
  onCommand: (command: ToolbarCommand) => void;
  disabled?: boolean;
}) {
  return (
    <div
      role="toolbar"
      aria-label="ابزار قالب‌بندی"
      className="flex flex-wrap items-center gap-1 border-b border-[var(--adm-border)] pb-2"
    >
      {BUTTONS.map((button) => (
        <button
          key={button.command}
          type="button"
          title={button.title}
          aria-label={button.title}
          disabled={disabled}
          onClick={() => onCommand(button.command)}
          className="adm-btn adm-btn-ghost adm-focus min-w-8 px-2 py-1 text-[12px] font-bold"
        >
          {button.label}
        </button>
      ))}
    </div>
  );
}
