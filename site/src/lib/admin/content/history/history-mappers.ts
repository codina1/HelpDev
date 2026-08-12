import type {
  ContentRevisionDetail,
  ContentRevisionDetailRawDto,
  ContentRevisionListItem,
  ContentRevisionListItemRawDto,
  ContentRevisionPagedResult,
  ContentRevisionPagedResultRawDto,
  ContentRevisionSnapshot,
  ContentRevisionSnapshotRawDto,
} from "@/lib/admin/content/history/history-types";

export function mapContentRevisionListItem(raw: ContentRevisionListItemRawDto): ContentRevisionListItem {
  return {
    versionNumber: raw.versionNumber,
    createdByUserId: raw.createdByUserId,
    createdAtUtc: raw.createdAtUtc,
    changeReason: raw.changeReason,
  };
}

export function mapContentRevisionSnapshot(raw: ContentRevisionSnapshotRawDto): ContentRevisionSnapshot {
  return {
    title: raw.title,
    slug: raw.slug,
    body: raw.body,
    excerpt: raw.excerpt,
    coverImage: raw.coverImage,
    contentType: raw.contentType,
    seoMetadata: raw.seoMetadata,
  };
}

export function mapContentRevisionDetail(raw: ContentRevisionDetailRawDto): ContentRevisionDetail {
  return {
    contentId: raw.contentId,
    versionNumber: raw.versionNumber,
    snapshot: mapContentRevisionSnapshot(raw.snapshot),
    changeReason: raw.changeReason,
    createdByUserId: raw.createdByUserId,
    createdAtUtc: raw.createdAtUtc,
  };
}

export function mapContentRevisionPagedResult(
  raw: ContentRevisionPagedResultRawDto,
): ContentRevisionPagedResult {
  return {
    items: raw.items.map(mapContentRevisionListItem),
    page: raw.page,
    pageSize: raw.pageSize,
    totalCount: raw.totalCount,
    totalPages: raw.totalPages,
  };
}
