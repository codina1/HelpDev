import type { SeoMetadataDto } from "@/lib/api/content";

export const ADMIN_CONTENT_REVISION_PAGE_SIZE_DEFAULT = 10;
export const ADMIN_CONTENT_REVISION_PAGE_SIZES = [10, 20, 50] as const;
export type AdminContentRevisionPageSize = (typeof ADMIN_CONTENT_REVISION_PAGE_SIZES)[number];

/** Workspace list query (client-driven pagination). */
export type ContentRevisionListQuery = {
  page: number;
  pageSize: AdminContentRevisionPageSize;
};

export const DEFAULT_CONTENT_REVISION_LIST_QUERY: ContentRevisionListQuery = {
  page: 1,
  pageSize: ADMIN_CONTENT_REVISION_PAGE_SIZE_DEFAULT,
};

/* ------------------------------- Raw API DTOs ------------------------------ */

export type ContentRevisionListItemRawDto = {
  versionNumber: number;
  createdByUserId: string;
  createdAtUtc: string;
  changeReason: string | null;
};

export type ContentRevisionSnapshotRawDto = {
  title: string;
  slug: string;
  body: string;
  excerpt: string;
  coverImage: string | null;
  contentType: string;
  seoMetadata: SeoMetadataDto;
};

export type ContentRevisionDetailRawDto = {
  contentId: string;
  versionNumber: number;
  snapshot: ContentRevisionSnapshotRawDto;
  changeReason: string | null;
  createdByUserId: string;
  createdAtUtc: string;
};

export type ContentRevisionPagedResultRawDto = {
  items: ContentRevisionListItemRawDto[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

/* ------------------------------ View models -------------------------------- */

export type ContentRevisionListItem = {
  versionNumber: number;
  createdByUserId: string;
  createdAtUtc: string;
  changeReason: string | null;
};

export type ContentRevisionSnapshot = {
  title: string;
  slug: string;
  body: string;
  excerpt: string;
  coverImage: string | null;
  contentType: string;
  seoMetadata: SeoMetadataDto;
};

export type ContentRevisionDetail = {
  contentId: string;
  versionNumber: number;
  snapshot: ContentRevisionSnapshot;
  changeReason: string | null;
  createdByUserId: string;
  createdAtUtc: string;
};

export type ContentRevisionPagedResult = {
  items: ContentRevisionListItem[];
  page: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
};

export type RestoreContentRevisionPayload = {
  changeReason?: string | null;
};
