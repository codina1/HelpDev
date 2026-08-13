import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import Link from "next/link";

export const metadata: Metadata = { title: "شرایط استفاده" };

export default function TermsPage() {
  return (
    <>
      <PageHeader
        title="شرایط استفاده"
        description="شرایط استفاده HelpDev هنوز به‌صورت کامل منتشر نشده است."
      />
      <div className="ui-panel space-y-4 p-6">
        <p className="ui-body">
          این صفحه جای‌نگهدار شرایط استفاده است. متن حقوقی کامل اینجا ساخته نمی‌شود.
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
