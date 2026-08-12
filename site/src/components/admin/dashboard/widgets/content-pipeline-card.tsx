import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import type { ContentPipeline } from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";

type ContentPipelineCardProps = {
  pipeline: AsyncSection<ContentPipeline>;
  onRetry: () => void;
};

/**
 * Section 2 (left) — content lifecycle. Only states the backend actually
 * exposes are shown (Draft, Published). Scheduled/Review/SEO are future states
 * and are intentionally not fabricated.
 */
export function ContentPipelineCard({ pipeline, onRetry }: ContentPipelineCardProps) {
  const data = pipeline.data;
  const total = data ? data.total : 0;
  const publishedPct = total > 0 && data ? Math.round((data.published / total) * 100) : 0;
  const draftPct = total > 0 && data ? Math.max(0, 100 - publishedPct) : 0;

  return (
    <WidgetCard
      title="نمای کلی محتوا"
      icon="content"
      loading={pipeline.loading}
      error={pipeline.error}
      isEmpty={!pipeline.loading && !pipeline.error && total === 0}
      emptyTitle="محتوایی وجود ندارد"
      emptyDescription="هنوز محتوایی ثبت نشده است."
      emptyIcon="content"
      onRetry={onRetry}
      className="h-full"
    >
      {data ? (
        <div className="space-y-5">
          <div className="flex items-end justify-between">
            <div>
              <p className="adm-muted text-[12px]">مجموع محتوا</p>
              <p className="adm-text text-3xl font-black tabular-nums">
                {formatNumberFa(data.total)}
              </p>
            </div>
          </div>

          <div
            className="flex h-2.5 w-full overflow-hidden rounded-full bg-[var(--adm-surface-3)]"
            role="img"
            aria-label={`${publishedPct} درصد منتشرشده`}
          >
            <span
              className="h-full bg-[var(--adm-success)]"
              style={{ inlineSize: `${publishedPct}%` }}
            />
            <span
              className="h-full bg-[var(--adm-warning)]"
              style={{ inlineSize: `${draftPct}%` }}
            />
          </div>

          <ul className="space-y-2.5">
            <PipelineRow
              label="منتشرشده"
              value={data.published}
              colorVar="--adm-success"
            />
            <PipelineRow
              label="پیش‌نویس"
              value={data.draft}
              colorVar="--adm-warning"
            />
          </ul>
        </div>
      ) : null}
    </WidgetCard>
  );
}

function PipelineRow({
  label,
  value,
  colorVar,
}: {
  label: string;
  value: number;
  colorVar: string;
}) {
  return (
    <li className="flex items-center justify-between gap-3 rounded-lg bg-[var(--adm-surface-2)] px-3 py-2">
      <span className="flex items-center gap-2">
        <span
          aria-hidden
          className="h-2.5 w-2.5 rounded-full"
          style={{ backgroundColor: `var(${colorVar})` }}
        />
        <span className="adm-text text-[13px] font-medium">{label}</span>
      </span>
      <span className="adm-text text-[15px] font-bold tabular-nums">
        {formatNumberFa(value)}
      </span>
    </li>
  );
}
