import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import { SITE } from "@/lib/constants";

export const metadata: Metadata = {
  title: "تماس",
  description: `ارتباط با ${SITE.name} — فرم و سرویس تماس هنوز فعال نیست.`,
};

export default function ContactPage() {
  return (
    <>
      <PageHeader
        title="تماس"
        description="این صفحه جای‌نگهدار ارتباط با HelpDev است. فرم یا API تماس هنوز فعال نیست."
      />
      <div className="ui-panel space-y-4 p-6">
        <p className="ui-body">
          مسیر عمومی تماس آماده است. ارسال پیام و اتصال به سرویس در این نسخه ساخته
          نشده.
        </p>
      </div>
    </>
  );
}
