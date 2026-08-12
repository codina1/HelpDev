export type SeoDashboardRawDto = {
  totalContent: number;
  publishedContent: number;
  missingSeoTitleCount: number;
  missingSeoDescriptionCount: number;
  missingCoverImageCount: number;
  missingCanonicalCount: number;
  lastAnalysisTime: string | null;
  criticalFindings: SeoDashboardCriticalFindingRawDto[];
  recentContent: SeoDashboardRecentContentRawDto[];
};

export type SeoDashboardCriticalFindingRawDto = {
  contentId: string;
  title: string;
  issueCode: string;
  message: string;
};

export type SeoDashboardRecentContentRawDto = {
  contentId: string;
  title: string;
  status: string;
  updatedAtUtc: string;
  missingSeoTitle: boolean;
  missingSeoDescription: boolean;
};

export type SeoDashboard = SeoDashboardRawDto;
