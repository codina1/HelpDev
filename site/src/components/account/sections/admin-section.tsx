"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import { useAuth } from "@/components/auth";
import { AdminUserModal } from "@/components/account/sections/admin-user-modal";
import {
  fetchAdminContentSummary,
  fetchAdminDashboard,
  fetchAdminUserDetail,
  fetchAdminUsersList,
  updateAdminUser,
  type AdminContentResponse,
  type AdminDashboard,
  type AdminUserDetail,
  type AdminUserListItem,
  type UpdateAdminUserRequest,
} from "@/lib/profile-api";

type AdminView = "overview" | "users" | "content";

export function AdminSection() {
  const { user, token } = useAuth();
  const [view, setView] = useState<AdminView>("overview");
  const [dashboard, setDashboard] = useState<AdminDashboard | null>(null);
  const [users, setUsers] = useState<AdminUserListItem[]>([]);
  const [contentData, setContentData] = useState<AdminContentResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  const [modalOpen, setModalOpen] = useState(false);
  const [modalMode, setModalMode] = useState<"view" | "edit">("view");
  const [selectedUser, setSelectedUser] = useState<AdminUserDetail | null>(null);
  const [modalLoading, setModalLoading] = useState(false);
  const [modalSaving, setModalSaving] = useState(false);
  const [modalError, setModalError] = useState<string | null>(null);

  const loadOverview = useCallback(async () => {
    if (!token || user?.role !== "Admin") return;

    setLoading(true);
    setError(null);

    try {
      const data = await fetchAdminDashboard(token);
      setDashboard(data);
      setView("overview");
    } catch (err) {
      setDashboard(null);
      setError(err instanceof Error ? err.message : "خطا در بارگذاری پنل.");
    } finally {
      setLoading(false);
    }
  }, [token, user?.role]);

  const openUsers = useCallback(async () => {
    if (!token) return;

    setView("users");
    setLoading(true);
    setError(null);

    try {
      const data = await fetchAdminUsersList(token);
      setUsers(data);
    } catch (err) {
      setUsers([]);
      setError(err instanceof Error ? err.message : "خطا در دریافت کاربران.");
    } finally {
      setLoading(false);
    }
  }, [token]);

  const openContent = useCallback(async () => {
    if (!token) return;

    setView("content");
    setLoading(true);
    setError(null);

    try {
      const data = await fetchAdminContentSummary(token);
      setContentData(data);
    } catch (err) {
      setContentData(null);
      setError(err instanceof Error ? err.message : "خطا در دریافت محتوا.");
    } finally {
      setLoading(false);
    }
  }, [token]);

  const openUserModal = useCallback(
    async (userId: string, mode: "view" | "edit") => {
      if (!token) return;

      setModalOpen(true);
      setModalMode(mode);
      setModalLoading(true);
      setModalError(null);
      setSelectedUser(null);

      try {
        const detail = await fetchAdminUserDetail(token, userId);
        setSelectedUser(detail);
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

  useEffect(() => {
    void loadOverview();
  }, [loadOverview]);

  if (user?.role !== "Admin") {
    return (
      <div className="dash-card p-6 text-[13px] text-amber-300">
        فقط کاربران ادمین به این بخش دسترسی دارند.
      </div>
    );
  }

  if (loading && view === "overview" && !dashboard) {
    return (
      <div className="dash-card p-6 text-[13px] text-slate-400">
        در حال بارگذاری پنل ادمین...
      </div>
    );
  }

  if (error && view === "overview" && !dashboard) {
    return (
      <div className="dash-card space-y-4 p-6">
        <p className="text-[13px] text-red-400">{error}</p>
        <button
          type="button"
          onClick={() => void loadOverview()}
          className="focus-ring rounded-xl bg-violet-600 px-4 py-2 text-[12px] font-bold text-white hover:bg-violet-500"
        >
          تلاش مجدد
        </button>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-start justify-between gap-3">
        <div>
          <h1 className="text-xl font-extrabold text-white">
            {view === "users"
              ? "مدیریت کاربران"
              : view === "content"
                ? "مدیریت محتوا"
                : "پنل ادمین"}
          </h1>
          <p className="mt-1 text-[13px] text-slate-400">
            {view === "overview"
              ? "نمای کلی وضعیت سامانه"
              : view === "users"
                ? "مشاهده و ویرایش اطلاعات کاربران"
                : "آمار محتوای منتشرشده"}
          </p>
        </div>

        {view !== "overview" && (
          <button
            type="button"
            onClick={() => {
              setError(null);
              setView("overview");
            }}
            className="focus-ring rounded-xl border border-white/10 px-3 py-2 text-[12px] font-semibold text-slate-300 hover:bg-white/[0.04]"
          >
            ← بازگشت به داشبورد
          </button>
        )}
      </div>

      {view === "overview" && dashboard && (
        <>
          <div className="grid gap-4 sm:grid-cols-3">
            <StatCard title="کاربران" value={dashboard.users?.totalUsers ?? 0} />
            <StatCard
              title="محتوای منتشرشده"
              value={dashboard.content?.publishedContent ?? 0}
            />
            <StatCard title="نقش شما" value={user?.role ?? "Admin"} />
          </div>

          <div className="grid gap-6 lg:grid-cols-[2fr_1fr]">
            <div className="dash-card p-6">
              <h2 className="text-[16px] font-bold text-white">مدیریت سریع</h2>
              <div className="mt-4 grid gap-3 sm:grid-cols-2">
                <ActionButton
                  title="مدیریت کاربران"
                  description="مشاهده و ویرایش کاربران"
                  onClick={() => void openUsers()}
                />
                <ActionButton
                  title="مدیریت محتوا"
                  description="بررسی آمار محتوای منتشرشده"
                  onClick={() => void openContent()}
                />
                <LinkAction
                  title="ایجاد محتوا"
                  description="صفحه نویسنده (به‌زودی کامل می‌شود)"
                  href="/write"
                />
                <a
                  href="http://localhost:5221/swagger"
                  target="_blank"
                  rel="noreferrer"
                  className="focus-ring block rounded-xl border border-white/10 bg-white/[0.03] px-4 py-4 transition-colors hover:border-amber-500/30 hover:bg-amber-500/10"
                >
                  <p className="text-[14px] font-bold text-white">Swagger API</p>
                  <p className="mt-1 text-[12px] text-slate-400">
                    تست endpointها با Authorize
                  </p>
                </a>
              </div>
            </div>

            <div className="dash-card p-6">
              <h2 className="text-[16px] font-bold text-white">وضعیت محتوا</h2>
              <ul className="mt-4 space-y-2">
                <OverviewRow
                  label="کل محتوا"
                  value={dashboard.content?.totalContent ?? 0}
                />
                <OverviewRow
                  label="منتشرشده"
                  value={dashboard.content?.publishedContent ?? 0}
                />
                <OverviewRow
                  label="پیش‌نویس"
                  value={dashboard.content?.draftContent ?? 0}
                />
                <OverviewRow
                  label="کاربران فعال"
                  value={dashboard.users?.activeUsers ?? 0}
                />
                <OverviewRow
                  label="اسناد جستجو"
                  value={dashboard.search?.publishedSearchDocuments ?? 0}
                />
              </ul>
            </div>
          </div>
        </>
      )}

      {view === "users" && (
        <div className="dash-card p-5 sm:p-6">
          {loading && (
            <p className="text-[13px] text-slate-400">در حال دریافت کاربران...</p>
          )}
          {error && !loading && (
            <div className="space-y-3">
              <p className="text-[13px] text-red-400">{error}</p>
              <button
                type="button"
                onClick={() => void openUsers()}
                className="focus-ring rounded-xl bg-violet-600 px-4 py-2 text-[12px] font-bold text-white"
              >
                تلاش مجدد
              </button>
            </div>
          )}
          {!loading && !error && (
            <>
              <p className="mb-4 text-[13px] text-slate-400">
                مجموع کاربران:{" "}
                <span className="font-bold text-white">{users.length}</span>
              </p>
              <div className="overflow-x-auto">
                <table className="w-full min-w-[640px] text-start text-[13px]">
                  <thead>
                    <tr className="border-b border-white/10 text-slate-500">
                      <th className="px-2 py-2 font-semibold">نام</th>
                      <th className="px-2 py-2 font-semibold">موبایل</th>
                      <th className="px-2 py-2 font-semibold">نقش</th>
                      <th className="px-2 py-2 font-semibold">عملیات</th>
                    </tr>
                  </thead>
                  <tbody>
                    {users.map((row) => (
                      <tr key={row.id} className="border-b border-white/[0.06]">
                        <td className="px-2 py-3 text-slate-200">
                          {row.displayName || "—"}
                        </td>
                        <td dir="ltr" className="px-2 py-3 text-slate-300">
                          {row.mobile}
                        </td>
                        <td className="px-2 py-3">
                          <RolePill role={row.role} />
                        </td>
                        <td className="px-2 py-3">
                          <div className="flex flex-wrap gap-2">
                            <button
                              type="button"
                              onClick={() => void openUserModal(row.id, "view")}
                              className="focus-ring rounded-lg border border-white/10 px-2.5 py-1 text-[11px] font-semibold text-slate-300 hover:bg-white/[0.05]"
                            >
                              مشاهده
                            </button>
                            <button
                              type="button"
                              onClick={() => void openUserModal(row.id, "edit")}
                              className="focus-ring rounded-lg border border-violet-500/30 bg-violet-500/10 px-2.5 py-1 text-[11px] font-semibold text-violet-200 hover:bg-violet-500/20"
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
            </>
          )}
        </div>
      )}

      {view === "content" && (
        <div className="dash-card p-5 sm:p-6">
          {loading && (
            <p className="text-[13px] text-slate-400">در حال دریافت آمار محتوا...</p>
          )}
          {error && !loading && (
            <div className="space-y-3">
              <p className="text-[13px] text-red-400">{error}</p>
              <button
                type="button"
                onClick={() => void openContent()}
                className="focus-ring rounded-xl bg-violet-600 px-4 py-2 text-[12px] font-bold text-white"
              >
                تلاش مجدد
              </button>
            </div>
          )}
          {!loading && !error && contentData && (
            <>
              <p className="mb-4 text-[13px] text-slate-400">
                محتوای منتشرشده:{" "}
                <span className="font-bold text-white">
                  {contentData.totalPublished}
                </span>
              </p>
              <ul className="space-y-2">
                {contentData.byType.map((item) => (
                  <li
                    key={item.type}
                    className="flex items-center justify-between rounded-xl border border-white/10 px-4 py-3 text-[13px]"
                  >
                    <span className="text-slate-300">{item.type}</span>
                    <span className="text-lg font-black text-white">{item.count}</span>
                  </li>
                ))}
              </ul>
            </>
          )}
        </div>
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

function OverviewRow({ label, value }: { label: string; value: string | number }) {
  return (
    <li className="flex items-center justify-between rounded-lg border border-white/10 px-3 py-2 text-[13px]">
      <span className="text-slate-300">{label}</span>
      <span className="font-bold text-white">{value}</span>
    </li>
  );
}

function StatCard({ title, value }: { title: string; value: string | number }) {
  return (
    <div className="dash-card p-5">
      <p className="text-[13px] text-slate-400">{title}</p>
      <p className="mt-2 text-3xl font-black text-white">{value}</p>
    </div>
  );
}

function ActionButton({
  title,
  description,
  onClick,
}: {
  title: string;
  description: string;
  onClick: () => void;
}) {
  return (
    <button
      type="button"
      onClick={onClick}
      className="focus-ring rounded-xl border border-white/10 bg-white/[0.03] px-4 py-4 text-start transition-colors hover:border-amber-500/30 hover:bg-amber-500/10"
    >
      <p className="text-[14px] font-bold text-white">{title}</p>
      <p className="mt-1 text-[12px] text-slate-400">{description}</p>
    </button>
  );
}

function LinkAction({
  title,
  description,
  href,
}: {
  title: string;
  description: string;
  href: string;
}) {
  return (
    <Link
      href={href}
      className="focus-ring block rounded-xl border border-white/10 bg-white/[0.03] px-4 py-4 transition-colors hover:border-amber-500/30 hover:bg-amber-500/10"
    >
      <p className="text-[14px] font-bold text-white">{title}</p>
      <p className="mt-1 text-[12px] text-slate-400">{description}</p>
    </Link>
  );
}

function RolePill({ role }: { role: string }) {
  const styles =
    role === "Admin"
      ? "bg-amber-400/15 text-amber-300 border-amber-400/30"
      : role === "Writer"
        ? "bg-cyan-500/15 text-cyan-300 border-cyan-500/30"
        : "bg-slate-500/15 text-slate-300 border-slate-500/25";

  return (
    <span className={`rounded-md border px-2 py-0.5 text-[11px] font-bold ${styles}`}>
      {role}
    </span>
  );
}
