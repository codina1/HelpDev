import { ProfileSummaryCard } from "@/components/account/dashboard/profile-summary-card";
import { LearningProgressCard } from "@/components/account/dashboard/learning-progress-card";
import { ContentPreferencesCard } from "@/components/account/dashboard/content-preferences-card";
import { SavedItemsCard } from "@/components/account/dashboard/saved-items-card";
import { RecentActivityCard } from "@/components/account/dashboard/recent-activity-card";
import { AnalyticsSection } from "@/components/account/dashboard/analytics-section";
import type { AuthUser } from "@/types/auth";

type DashboardHomeProps = {
  user: AuthUser;
};

export function DashboardHome({ user }: DashboardHomeProps) {
  return (
    <div className="space-y-5">
      <ProfileSummaryCard user={user} />

      <div className="grid gap-5 lg:grid-cols-2">
        <LearningProgressCard />
        <ContentPreferencesCard />
      </div>

      <div className="grid gap-5 lg:grid-cols-2">
        <SavedItemsCard />
        <RecentActivityCard />
      </div>

      <AnalyticsSection />
    </div>
  );
}
