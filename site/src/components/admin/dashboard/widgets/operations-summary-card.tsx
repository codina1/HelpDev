import type { ReactNode } from "react";
import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import type {
  OperationalStatus,
  OperationsSummary,
} from "@/lib/admin/dashboard/dashboard-types";
import type { AsyncSection } from "@/lib/admin/dashboard/dashboard-hooks";
import { WidgetCard } from "@/components/admin/dashboard/widgets/widget-card";
import { StatusBadge } from "@/components/admin/dashboard/widgets/status-badge";

type OperationsSummaryCardProps = {
  operations: AsyncSection<OperationsSummary>;
  onRetry: () => void;
};

/** Small operational widget summarizing Outbox, Search, Analytics and Audit. */
export function OperationsSummaryCard({
  operations,
  onRetry,
}: OperationsSummaryCardProps) {
  const data = operations.data;

  return (
    <WidgetCard
      title="خلاصه عملیات"
      icon="outbox"
      headerAction={data ? <StatusBadge status={data.overall} /> : undefined}
      loading={operations.loading}
      error={operations.error}
      onRetry={onRetry}
      className="h-full"
    >
      {data ? (
        <ul className="grid grid-cols-2 gap-2">
          <OperationTile
            label="Outbox"
            status={data.outbox.status}
            metric={`${formatNumberFa(data.outbox.pending)} در صف`}
            hint={data.outbox.failed > 0 ? `${formatNumberFa(data.outbox.failed)} ناموفق` : undefined}
          />
          <OperationTile
            label="جستجو"
            status={data.search.status}
            metric={`${formatNumberFa(data.search.pending)} در انتظار`}
          />
          <OperationTile
            label="تحلیل‌ها"
            status={data.analytics.status}
            metric={`${formatNumberFa(data.analytics.recentProcessed)} پردازش‌شده`}
          />
          <OperationTile
            label="Audit"
            status={data.audit.status}
            metric={data.audit.available ? "در دسترس" : "قطع"}
          />
        </ul>
      ) : null}
    </WidgetCard>
  );
}

function OperationTile({
  label,
  status,
  metric,
  hint,
}: {
  label: string;
  status: OperationalStatus;
  metric: string;
  hint?: ReactNode;
}) {
  return (
    <li className="space-y-1.5 rounded-lg bg-[var(--adm-surface-2)] p-3">
      <div className="flex items-center justify-between gap-2">
        <span className="adm-text text-[12px] font-bold">{label}</span>
        <StatusBadge status={status} />
      </div>
      <p className="adm-text text-[13px] font-semibold tabular-nums">{metric}</p>
      {hint ? <p className="text-[11px] text-[var(--adm-danger)]">{hint}</p> : null}
    </li>
  );
}
