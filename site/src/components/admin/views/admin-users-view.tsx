"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import {
  fetchAdminUserDetail,
  fetchAdminUsersList,
  updateAdminUser,
  type AdminUserDetail,
  type AdminUserListItem,
  type UpdateAdminUserRequest,
} from "@/lib/profile-api";
import { useAuth } from "@/components/auth";
import { AdminUserModal } from "@/components/account/sections/admin-user-modal";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { AdminActionBar } from "@/components/admin/page/admin-action-bar";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminEmptyState } from "@/components/admin/feedback/admin-empty-state";

const ROLE_STYLE: Record<string, string> = {
  Admin: "bg-[var(--adm-warning-soft)] text-[var(--adm-warning)]",
  Writer: "bg-[var(--adm-info-soft)] text-[var(--adm-info)]",
  User: "bg-[var(--adm-surface-3)] text-[var(--adm-text-muted)]",
};

export function AdminUsersView() {
  const { token } = useAuth();
  const [users, setUsers] = useState<AdminUserListItem[]>([]);
  const [error, setError] = useState<unknown>(null);
  const [loading, setLoading] = useState(true);
  const [query, setQuery] = useState("");

  const [modalOpen, setModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<"view" | "edit">("view");
  const [selectedUser, setSelectedUser] = useState<AdminUserDetail | null>(null);
  const [modalLoading, setModalLoading] = useState(false);
  const [modalSaving, setModalSaving] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);

  const load = useCallback(async () => {
    if (!token) return;
    setLoading(true);
    setError(null);
    try {
      setUsers(await fetchAdminUsersList(token));
    } catch (err) {
      setError(err);
      setUsers([]);
    } finally {
      setLoading(false);
    }
  }, [token]);

  useEffect(() => {
    void load();
  }, [load]);

  const filtered = useMemo(() => {
    const q = query.trim().toLowerCase();
    if (!q) return users;
    return users.filter((user) =>
      [user.displayName, user.mobile, user.email, user.role]
        .join(" ")
        .toLowerCase()
        .includes(q),
    );
  }, [users, query]);

  const openUserModal = useCallback(
    async (userId: string, mode: "view" | "edit") => {
      if (!token) return;
      setModalOpen(true);
      setModalMode(mode);
      setModalLoading(true);
      setModalError(null);
      setSelectedUser(null);
      try {
        setSelectedUser(await fetchAdminUserDetail(token, userId));
      } catch (err) {
        setModalError(err instanceof Error ? err.message : "خطا در دریافت کاربر.");
      } finally {
        setModalLoading(false);
      }
    },
    [token],
  );

  const handleSaveUser = useCallback(
    async (request: UpdateAdminUserRequest) => {
      if (!token || !selectedUser) return;
      setModalSaving(true);
      setModalError(null);
      try {
        const updated = await updateAdminUser(token, selectedUser.id, request);
        setSelectedUser(updated);
        setUsers((prev) =>
          prev.map((item) =>
            item.id === updated.id
              ? {
                  ...item,
                  firstName: updated.firstName,
                  lastName: updated.lastName,
                  displayName: updated.displayName,
                  email: updated.email,
                  role: updated.role,
                }
              : item,
          ),
        );
        setModalMode("view");
      } catch (err) {
        setModalError(err instanceof Error ? err.message : "ذخیره ناموفق بود.");
      } finally {
        setModalSaving(false);
      }
    },
    [token, selectedUser],
  );

  return (
    <div className="space-y-5">
      <AdminPageHeader
        title="مدیریت کاربران"
        description="مشاهده، جستجو و ویرایش اطلاعات کاربران سامانه."
        meta={loading ? undefined : `${users.length} کاربر`}
      />

      <AdminActionBar
        filters={
          <input
            type="search"
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="جستجوی نام، موبایل یا ایمیل..."
            className="adm-input max-w-xs"
            aria-label="جستجوی کاربران"
          />
        }
      />

      {loading ? (
        <AdminLoadingState cards={0} rows={6} />
      ) : error ? (
        <AdminErrorState error={error} onRetry={() => void load()} />
      ) : filtered.length === 0 ? (
        <AdminEmptyState
          icon="users"
          title={query ? "کاربری با این فیلتر پیدا نشد" : "کاربری وجود ندارد"}
          description={query ? "عبارت جستجو را تغییر دهید." : undefined}
        />
      ) : (
        <AdminSurface padding="none">
          <div className="adm-scroll overflow-x-auto">
            <table className="w-full min-w-[640px] text-start text-[13px]">
              <thead>
                <tr className="adm-border-b adm-subtle text-[11px]">
                  <th className="px-4 py-3 text-start font-semibold">نام</th>
                  <th className="px-4 py-3 text-start font-semibold">موبایل</th>
                  <th className="px-4 py-3 text-start font-semibold">نقش</th>
                  <th className="px-4 py-3 text-start font-semibold">عملیات</th>
                </tr>
              </thead>
              <tbody className="adm-divide">
                {filtered.map((row) => (
                  <tr key={row.id} className="adm-hover">
                    <td className="adm-text px-4 py-3">{row.displayName || "—"}</td>
                    <td dir="ltr" className="adm-muted px-4 py-3 text-start">{row.mobile}</td>
                    <td className="px-4 py-3">
                      <span className={`rounded-md px-2 py-0.5 text-[11px] font-bold ${ROLE_STYLE[row.role] ?? ROLE_STYLE.User}`}>
                        {row.role}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-2">
                        <button
                          type="button"
                          onClick={() => void openUserModal(row.id, "view")}
                          className="adm-btn adm-btn-ghost adm-focus px-2.5 py-1 text-[11px]"
                        >
                          مشاهده
                        </button>
                        <button
                          type="button"
                          onClick={() => void openUserModal(row.id, "edit")}
                          className="adm-btn adm-btn-outline adm-focus px-2.5 py-1 text-[11px]"
                        >
                          ویرایش
                        </button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        </AdminSurface>
      )}

      <AdminUserModal
        open={modalOpen}
        mode={modalMode}
        user={selectedUser}
        loading={modalLoading}
        saving={modalSaving}
        error={modalError}
        onClose={() => {
          setModalOpen(false);
          setModalError(null);
        }}
        onSwitchToEdit={() => setModalMode("edit")}
        onSave={handleSaveUser}
      />
    </div>
  );
}
