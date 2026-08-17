import { WRITER_PROMPT_STATUSES } from "@/lib/admin/prompt-lab/writer-prompt-types";
import { labelForWriterPromptStatus } from "@/lib/admin/prompt-lab/writer-prompt-mappers";
import { WriterPromptStatusBadge } from "@/components/admin/prompt-lab/writer-prompt-status-badge";

/** Status legend for writer prompt workflow. */
export function WriterPromptStatusLegend() {
  return (
    <div
      className="flex flex-wrap items-center gap-2"
      role="list"
      aria-label="راهنمای وضعیت پرامپت"
    >
      {WRITER_PROMPT_STATUSES.map((status) => (
        <div key={status} className="flex items-center gap-1.5" role="listitem">
          <WriterPromptStatusBadge status={status} />
          <span className="sr-only">{labelForWriterPromptStatus(status)}</span>
        </div>
      ))}
    </div>
  );
}
