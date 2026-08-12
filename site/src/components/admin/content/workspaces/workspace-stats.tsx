import { formatNumberFa } from "@/lib/admin/dashboard/dashboard-mappers";
import { AdminPageSection } from "@/components/admin/page/admin-page-section";
import { AdminStatCard } from "@/components/admin/page/admin-stat-card";
import type { ContentWorkspaceDefinition } from "@/lib/admin/content/factory";

type WorkspaceStatsProps = {
  workspace: ContentWorkspaceDefinition;
  matchingCount: number | null;
  loading?: boolean;
};

/** Lightweight stats strip for a typed workspace (uses filtered totalCount). */
export function WorkspaceStats({
  workspace,
  matchingCount,
  loading = false,
}: WorkspaceStatsProps) {
  return (
    <AdminPageSection title="آمار فضای کار">
      <div className="grid grid-cols-1 gap-4 sm:grid-cols-2 lg:grid-cols-3">
        <AdminStatCard
          label={workspace.title}
          icon={workspace.icon}
          tone="info"
          value={matchingCount != null ? formatNumberFa(matchingCount) : "—"}
          loading={loading}
        />
        <AdminStatCard
          label="نوع محتوا (بک‌اند)"
          icon="tag"
          tone="neutral"
          value={workspace.contentType === "none" ? "—" : workspace.contentType}
          loading={false}
        />
        <AdminStatCard
          label="قابلیت‌های آینده"
          icon="flag"
          tone="warning"
          value={
            workspace.futureCapabilities?.length
              ? formatNumberFa(workspace.futureCapabilities.length)
              : "—"
          }
          loading={false}
        />
      </div>
      <p className="adm-subtle mt-2 text-[11px]">
        شمارش از فیلتر نوع محتوا روی API فهرست ادمین است — بدون دادهٔ ساختگی.
      </p>
    </AdminPageSection>
  );
}
