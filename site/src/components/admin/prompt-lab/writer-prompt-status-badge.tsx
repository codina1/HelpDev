import {
  labelForWriterPromptStatus,
  WRITER_PROMPT_STATUS_BADGE_CLASS,
} from "@/lib/admin/prompt-lab/writer-prompt-mappers";
import type { WriterPromptStatus } from "@/lib/admin/prompt-lab/writer-prompt-types";

/** Semantic badge for Prompt Lab writer workflow status. */
export function WriterPromptStatusBadge({ status }: { status: WriterPromptStatus }) {
  const tone = WRITER_PROMPT_STATUS_BADGE_CLASS[status] ?? WRITER_PROMPT_STATUS_BADGE_CLASS.Draft;
  return (
    <span
      className={`inline-flex items-center gap-1.5 rounded-md px-2 py-0.5 text-[11px] font-bold ${tone}`}
    >
      <span aria-hidden className="h-1.5 w-1.5 rounded-full bg-current" />
      {labelForWriterPromptStatus(status)}
    </span>
  );
}
