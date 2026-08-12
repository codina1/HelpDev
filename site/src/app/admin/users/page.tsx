import type { Metadata } from "next";
import { AdminUsersView } from "@/components/admin/views/admin-users-view";

export const metadata: Metadata = { title: "کاربران" };

export default function AdminUsersPage() {
  return <AdminUsersView />;
}
