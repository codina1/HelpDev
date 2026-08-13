import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import Link from "next/link";

export const metadata: Metadata = { title: "حریم خصوصی" };

export default function PrivacyPage() {
  return (
    <>
      <PageHeader
        title="حریم خصوصی"
        description="سیاست حریم خصوصی HelpDev هنوز به‌صورت کامل منتشر نشده است."
      />
      <div className="ui-panel space-y-4 p-6">
        <p className="ui-body">
          این صفحه جای‌نگهدار سیاست حریم خصوصی است. جزئیات جمع‌آوری و نگهداری داده
          اینجا جعل نمی‌شود.
        </p>
        <p className="ui-body">
          برای پرسش، از{" "}
          <Link href="/settings" className="text-[color:var(--home-cyan)]">
            صفحه تماس
          </Link>{" "}
          استفاده کنید.
        </p>
      </div>
    </>
  );
}
