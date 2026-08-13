import type { Metadata } from "next";
import { PageHeader } from "@/components/layout";
import { SITE } from "@/lib/constants";

export const metadata: Metadata = {
  title: "درباره ما",
  description: SITE.description,
};

export default function AboutPage() {
  return (
    <>
      <PageHeader title="درباره ما" description={SITE.description} />
      <div className="ui-panel space-y-4 p-6">
        <p className="ui-body">
          هلپ‌دو یک پلتفرم دانش مهندسی هوش مصنوعی است؛ جایی برای مقالات، ابزارها،
          نقشه راه و دستیار یادگیری تا مسیر اجرا کوتاه‌تر شود.
        </p>
        <p className="ui-body">
          این صفحه عمومی است و به تنظیمات حساب یا پیکربندی مدیریت وصل نیست.
        </p>
      </div>
    </>
  );
}
