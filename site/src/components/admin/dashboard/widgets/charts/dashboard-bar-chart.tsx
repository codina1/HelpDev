import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";

export type BarDatum = {
  label: string;
  value: number;
};

type DashboardBarChartProps = {
  data: BarDatum[];
  colorVar?: string;
  ariaLabel: string;
  height?: number;
};

/**
 * Dependency-free SVG bar chart for real snapshot values (composition), not
 * fabricated time-series. RTL-aware: the first datum renders at the inline-start
 * (right) edge. Renders nothing meaningful when all values are zero — callers
 * should show an empty state in that case.
 */
export function DashboardBarChart({
  data,
  colorVar = "--adm-accent",
  ariaLabel,
  height = 160,
}: DashboardBarChartProps) {
  const width = 320;
  const paddingBottom = 28;
  const paddingTop = 20;
  const chartHeight = height - paddingBottom - paddingTop;
  const max = Math.max(1, ...data.map((d) => d.value));
  const slot = width / data.length;
  const barWidth = Math.min(56, slot * 0.5);

  return (
    <svg
      viewBox={`0 0 ${width} ${height}`}
      role="img"
      aria-label={ariaLabel}
      className="h-auto w-full"
      preserveAspectRatio="xMidYMid meet"
    >
      {data.map((datum, index) => {
        // RTL: first item on the right.
        const rtlIndex = data.length - 1 - index;
        const centerX = rtlIndex * slot + slot / 2;
        const barHeight = Math.max(2, (datum.value / max) * chartHeight);
        const x = centerX - barWidth / 2;
        const y = paddingTop + (chartHeight - barHeight);
        return (
          <g key={datum.label}>
            <rect
              x={x}
              y={y}
              width={barWidth}
              height={barHeight}
              rx={6}
              fill={`var(${colorVar})`}
            />
            <text
              x={centerX}
              y={y - 6}
              textAnchor="middle"
              fill="var(--adm-text)"
              fontSize="12"
              fontWeight="700"
            >
              {formatNumberFa(datum.value)}
            </text>
            <text
              x={centerX}
              y={height - 8}
              textAnchor="middle"
              fill="var(--adm-text-muted)"
              fontSize="11"
            >
              {datum.label}
            </text>
          </g>
        );
      })}
    </svg>
  );
}
