import type { Metadata } from "next";
import { UserDashboard } from "@/components/dashboard/user-dashboard";

export const metadata: Metadata = {
  title: "داشبورد کاربر",
};

export default function DashboardPage() {
  return <UserDashboard />;
}
