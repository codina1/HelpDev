import type { Metadata } from "next";
import { redirect } from "next/navigation";

export const metadata: Metadata = { title: "چیت‌شیت" };

export default function CheatSheetsPage() {
  redirect("/toolbox");
}
