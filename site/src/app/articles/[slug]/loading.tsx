import { ArticlesContainer } from "@/components/articles/articles-container";

function Skeleton({ className }: { className?: string }) {
  return <div className={["animate-pulse rounded-xl bg-white/[0.06]", className ?? ""].join(" ")} />;
}

export default function ArticleDetailLoading() {
  return (
    <div className="bg-[#050816] pb-12 pt-4" dir="rtl" aria-busy="true" aria-live="polite">
      <span className="sr-only">در حال بارگذاری مقاله</span>
      <ArticlesContainer>
        <Skeleton className="mb-5 h-4 w-72" />
        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[280px_minmax(0,1fr)_280px]"
        >
          <div className="hidden space-y-4 xl:block">
            <Skeleton className="h-64 rounded-2xl" />
            <Skeleton className="h-48 rounded-2xl" />
          </div>
          <div className="space-y-4">
            <Skeleton className="h-8 w-40" />
            <Skeleton className="h-12 w-full" />
            <Skeleton className="h-16 w-4/5" />
            <Skeleton className="h-[320px] w-full rounded-[20px]" />
            <Skeleton className="h-80 w-full rounded-2xl" />
          </div>
          <div className="space-y-4">
            <Skeleton className="h-52 rounded-2xl" />
            <Skeleton className="h-28 rounded-2xl" />
            <Skeleton className="h-40 rounded-2xl" />
          </div>
        </div>
      </ArticlesContainer>
    </div>
  );
}
