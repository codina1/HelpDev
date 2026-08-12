"use client";

import { useCallback, useRef } from "react";
import { EditorToolbar, type ToolbarCommand } from "@/components/admin/content/editor/editor-toolbar";
import { CharacterCounter } from "@/components/admin/content/editor/character-counter";

type WrapSpec = { before: string; after: string; placeholder: string; block?: boolean };

const COMMANDS: Record<ToolbarCommand, WrapSpec> = {
  bold: { before: "**", after: "**", placeholder: "متن پررنگ" },
  italic: { before: "*", after: "*", placeholder: "متن مورب" },
  code: { before: "`", after: "`", placeholder: "code" },
  link: { before: "[", after: "](https://)", placeholder: "متن پیوند" },
  heading: { before: "## ", after: "", placeholder: "عنوان", block: true },
  unordered: { before: "- ", after: "", placeholder: "مورد فهرست", block: true },
  ordered: { before: "1. ", after: "", placeholder: "مورد فهرست", block: true },
};

/**
 * Dependency-free Markdown editor: a textarea plus a formatting toolbar that
 * inserts Markdown around the current selection. No rich-text/WYSIWYG library.
 */
export function MarkdownEditor({
  value,
  onChange,
  disabled = false,
  error,
  ariaInvalid,
}: {
  value: string;
  onChange: (value: string) => void;
  disabled?: boolean;
  error?: string;
  ariaInvalid?: boolean;
}) {
  const ref = useRef<HTMLTextAreaElement | null>(null);

  const runCommand = useCallback(
    (command: ToolbarCommand) => {
      const textarea = ref.current;
      if (!textarea) return;
      const spec = COMMANDS[command];
      const start = textarea.selectionStart;
      const end = textarea.selectionEnd;
      const selected = value.slice(start, end);
      const inner = selected || spec.placeholder;
      const prefix = spec.block && start > 0 && value[start - 1] !== "\n" ? "\n" : "";
      const insertion = `${prefix}${spec.before}${inner}${spec.after}`;
      const next = value.slice(0, start) + insertion + value.slice(end);
      onChange(next);

      const selectionStart = start + prefix.length + spec.before.length;
      const selectionEnd = selectionStart + inner.length;
      requestAnimationFrame(() => {
        textarea.focus();
        textarea.setSelectionRange(selectionStart, selectionEnd);
      });
    },
    [value, onChange],
  );

  return (
    <div className="space-y-2">
      <EditorToolbar onCommand={runCommand} disabled={disabled} />
      <textarea
        ref={ref}
        className="adm-input min-h-[360px] resize-y font-mono text-[13px] leading-7"
        value={value}
        disabled={disabled}
        onChange={(event) => onChange(event.target.value)}
        aria-invalid={ariaInvalid}
        aria-label="متن محتوا (Markdown)"
      />
      <div className="flex items-center justify-between gap-2">
        {error ? (
          <p className="text-[11px] font-semibold text-[var(--adm-danger)]">{error}</p>
        ) : (
          <p className="adm-subtle text-[11px]">
            از Markdown پشتیبانی می‌شود: عنوان‌ها، فهرست‌ها، کد، پیوند و تأکید.
          </p>
        )}
        <CharacterCounter value={Array.from(value).length} />
      </div>
    </div>
  );
}
