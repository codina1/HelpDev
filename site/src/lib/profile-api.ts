import { apiRequest } from "@/lib/api/client";
import { fetchMyProfile as fetchMyProfileV1, updateMyProfile as updateMyProfileV1 } from "@/lib/api/profile";
import { getAuthHeaders } from "@/lib/auth-api";
import type { AuthUser, UpdateProfileRequest } from "@/types/auth";

export { getAuthHeaders };

function mapProfile(data: Record<string, unknown>): AuthUser {
  return {
    id: String(data.id),
    mobile: String(data.mobile),
    role: data.role as AuthUser["role"],
    firstName: String(data.firstName ?? ""),
    lastName: String(data.lastName ?? ""),
    displayName: String(data.displayName ?? data.mobile ?? ""),
    email: String(data.email ?? ""),
    profileImageUrl: String(data.profileImageUrl ?? ""),
    expertise: String(data.expertise ?? ""),
    interests: String(data.interests ?? ""),
    profileCompletionPercent: Number(data.profileCompletionPercent ?? 0),
  };
}

export async function fetchMyProfile(token: string): Promise<AuthUser> {
  const data = await fetchMyProfileV1(token);
  return mapProfile(data as Record<string, unknown>);
}

export async function updateMyProfile(
  token: string,
  request: UpdateProfileRequest,
): Promise<AuthUser> {
  const data = await updateMyProfileV1(token, request);
  return mapProfile(data as Record<string, unknown>);
}

export type AdminDashboard = {
  users: {
    totalUsers: number;
    activeUsers: number;
    registrationsToday: number;
  };
  content: {
    totalContent: number;
    publishedContent: number;
    draftContent: number;
    publicationsToday: number | null;
  };
  learning: {
    totalCourses: number;
    publishedCourses: number;
    totalEnrollments: number;
    enrollmentsToday: number;
  };
  search: {
    totalSearchDocuments: number;
    publishedSearchDocuments: number;
    lastIndexedAtUtc: string | null;
  };
  outbox: {
    pending: number;
    processing: number;
    failed: number;
    processed: number;
    oldestPendingAtUtc: string | null;
    lastProcessedAtUtc: string | null;
  };
  recentItems: Array<{
    category: string;
    id: string;
    title: string;
    occurredAtUtc: string;
  }>;
};

export function fetchAdminDashboard(token: string): Promise<AdminDashboard> {
  return apiRequest<AdminDashboard>({ path: "/admin/dashboard", token, cache: "no-store" });
}

export type AdminUserListItem = {
  id: string;
  mobile: string;
  firstName: string;
  lastName: string;
  displayName: string;
  email: string;
  role: string;
  createdAt: string;
  lastLogin: string | null;
};

export type AdminUserDetail = {
  id: string;
  mobile: string;
  role: string;
  firstName: string;
  lastName: string;
  displayName: string;
  email: string;
  profileImageUrl: string;
  expertise: string;
  interests: string;
  profileCompletionPercent: number;
  createdAt: string;
  lastLogin: string | null;
};

export type UpdateAdminUserRequest = {
  firstName: string;
  lastName: string;
  email: string;
  profileImageUrl: string;
  expertise: string;
  interests: string;
  role: string;
};

export type AdminUsersResponse = {
  status: string;
  total: number;
  users: Array<{ id: string; mobile: string; fullName: string; role: string }>;
};

export type AdminContentResponse = {
  status: string;
  totalPublished: number;
  byType: Array<{ type: string; count: number }>;
};

function mapAdminUserListItem(data: Record<string, unknown>): AdminUserListItem {
  return {
    id: String(data.id),
    mobile: String(data.mobile ?? ""),
    firstName: String(data.firstName ?? ""),
    lastName: String(data.lastName ?? ""),
    displayName: String(data.displayName ?? data.mobile ?? ""),
    email: String(data.email ?? ""),
    role: String(data.role ?? "User"),
    createdAt: String(data.createdAt ?? ""),
    lastLogin: data.lastLogin ? String(data.lastLogin) : null,
  };
}

function mapAdminUserDetail(data: Record<string, unknown>): AdminUserDetail {
  return {
    ...mapAdminUserListItem(data),
    profileImageUrl: String(data.profileImageUrl ?? ""),
    expertise: String(data.expertise ?? ""),
    interests: String(data.interests ?? ""),
    profileCompletionPercent: Number(data.profileCompletionPercent ?? 0),
  };
}

export async function fetchAdminUsersList(token: string): Promise<AdminUserListItem[]> {
  const data = await apiRequest<unknown[]>({ path: "/admin/users", token, cache: "no-store" });
  return data.map((item) => mapAdminUserListItem(item as Record<string, unknown>));
}

export async function fetchAdminUserDetail(
  token: string,
  userId: string,
): Promise<AdminUserDetail> {
  const data = await apiRequest<Record<string, unknown>>({
    path: `/admin/users/${encodeURIComponent(userId)}`,
    token,
    cache: "no-store",
  });
  return mapAdminUserDetail(data);
}

export async function updateAdminUser(
  token: string,
  userId: string,
  request: UpdateAdminUserRequest,
): Promise<AdminUserDetail> {
  const data = await apiRequest<Record<string, unknown>>({
    method: "PUT",
    path: `/admin/users/${encodeURIComponent(userId)}`,
    token,
    body: request,
    cache: "no-store",
  });
  return mapAdminUserDetail(data);
}

/** @deprecated Prefer fetchAdminUsersList — kept for backwards compatibility with test endpoint. */
export async function fetchAdminUsers(token: string): Promise<AdminUsersResponse> {
  const users = await fetchAdminUsersList(token);
  return {
    status: "Healthy",
    total: users.length,
    users: users.map((user) => ({
      id: user.id,
      mobile: user.mobile,
      fullName: user.displayName,
      role: user.role,
    })),
  };
}

export async function fetchAdminContentSummary(token: string): Promise<AdminContentResponse> {
  const data = await apiRequest<Record<string, unknown>>({
    path: "/test/content",
    token,
    cache: "no-store",
  });

  const byType = Array.isArray(data.byType) ? data.byType : [];

  return {
    status: String(data.status ?? ""),
    totalPublished: Number(data.totalPublished ?? 0),
    byType: byType.map((item) => {
      const row = item as Record<string, unknown>;
      return {
        type: String(row.type ?? ""),
        count: Number(row.count ?? 0),
      };
    }),
  };
}
