import type { ArticleDetailViewModel } from "@/data/article-detail";

type ArticleInfoProps = {
  model: Pick<
    ArticleDetailViewModel,
    "category" | "publishedAtLabel" | "readingTime" | "viewsLabel" | "displayAuthor"
  >;
};

export function ArticleInfo({ model }: ArticleInfoProps) {
  const rows = [
    { label: "دسته‌بندی", value: model.category },
    { label: "تاریخ انتشار", value: model.publishedAtLabel },
    { label: "زمان مطالعه", value: model.readingTime },
    { label: "بازدید", value: model.viewsLabel },
    { label: "نویسنده", value: model.displayAuthor },
  ];

  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4 shadow-[0_0_28px_rgba(139,92,246,0.1)] backdrop-blur-xl">
      <h2 className="text-[13px] font-extrabold text-white">اطلاعات مقاله</h2>
      <dl className="mt-3 space-y-2.5">
        {rows.map((row) => (
          <div key={row.label} className="flex items-center justify-between gap-3 text-[12.5px]">
            <dt className="text-[#64748B]">{row.label}</dt>
            <dd className="truncate font-bold text-[#E5E7EB]">{row.value}</dd>
          </div>
        ))}
      </dl>
    </aside>
  );
}
