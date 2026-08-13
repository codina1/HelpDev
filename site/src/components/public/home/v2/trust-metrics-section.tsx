import { PremiumSectionHeader } from "@/components/experience/premium-section-header";
import { Card } from "@/components/ui/ds/card";
import { PublicSection } from "@/components/ui/public/v2";

export type TrustMetric = {
  label: string;
  value: number;
  hint?: string;
};

type TrustMetricsSectionProps = {
  metrics: TrustMetric[];
};

function formatFa(n: number): string {
  return n.toLocaleString("fa-IR");
}

/**
 * Trust Metrics — counts from real published catalog lists (never invented totals).
 */
export function TrustMetricsSection({ metrics }: TrustMetricsSectionProps) {
  return (
    <PublicSection className="ds-fade" aria-labelledby="trust-metrics-title">
      <PremiumSectionHeader
        eyebrow="Platform"
        title="آمار منتشرشده"
        description="شاخص‌های واقعی از محتوای منتشرشده و ابزارهای در دسترس HelpDev"
        titleId="trust-metrics-title"
        icon={<span aria-hidden>◈</span>}
      />
      <div className="grid gap-3 sm:grid-cols-2 lg:grid-cols-4">
        {metrics.map((metric) => (
          <Card
            key={metric.label}
            variant="glass"
            hover={false}
            className="relative overflow-hidden text-center"
          >
            <div
              className="pointer-events-none absolute inset-x-0 top-0 h-px bg-gradient-to-l from-transparent via-[color:var(--ds-primary)]/50 to-transparent"
              aria-hidden
            />
            <p className="text-3xl font-extrabold tracking-tight text-[color:var(--ds-fg)] sm:text-4xl">
              {formatFa(metric.value)}
            </p>
            <p className="mt-2 text-[13px] font-bold text-[color:var(--ds-fg)]">{metric.label}</p>
            {metric.hint ? (
              <p className="mt-1 text-[11px] text-[color:var(--ds-muted)]">{metric.hint}</p>
            ) : null}
          </Card>
        ))}
      </div>
    </PublicSection>
  );
}
