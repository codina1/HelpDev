"use client";

import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { AdminStatCard } from "@/components/admin/page/admin-stat-card";
import type { AdminIconName, AdminNavTone } from "@/lib/admin/navigation";

type MetricCardProps = {
  label: string;
  value: number | null;
  description?: string;
  icon?: AdminIconName;
  tone?: AdminNavTone;
  emptyLabel?: string;
};

/** Single factual metric tile — never invents zeros as growth. */
export function MetricCard({
  label,
  value,
  description,
  icon = "analytics",
  tone = "neutral",
  emptyLabel = "—",
}: MetricCardProps) {
  return (
    <AdminStatCard
      label={label}
      value={value == null ? emptyLabel : formatNumberFa(value)}
      description={description}
      icon={icon}
      tone={tone}
    />
  );
}
