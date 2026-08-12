import { PageHeader } from "@/components/layout";

type PlaceholderPageProps = {
  title: string;
  description: string;
};

export function PlaceholderPage({ title, description }: PlaceholderPageProps) {
  return (
    <>
      <PageHeader title={title} description={description} />
      <div className="ui-panel p-6">
        <p className="ui-body">
          این بخش به‌زودی با محتوای کامل در دسترس قرار می‌گیرد. فعلاً از صفحه
          خانه و بخش‌های فعال پلتفرم استفاده کنید.
        </p>
      </div>
    </>
  );
}
