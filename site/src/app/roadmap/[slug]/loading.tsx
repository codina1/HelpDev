import { ArticlesContainer } from "@/components/articles/articles-container";

function Skeleton({ className }: { className?: string }) {
  return <div className={["animate-pulse rounded-xl bg-white/[0.06]", className ?? ""].join(" ")} />;
}

export default function RoadmapDetailLoading() {
  return (
    <div className="bg-[#050816] pb-12 pt-4" dir="rtl" aria-busy="true">
      <span className="sr-only">در حال بارگذاری نقشه راه</span>
      <ArticlesContainer>
        <Skeleton className="mb-5 h-4 w-80" />
        <Skeleton className="mb-6 h-[240px] w-full rounded-[22px]" />
        <div
          dir="ltr"
          className="grid grid-cols-1 gap-6 xl:grid-cols-[260px_minmax(0,1fr)_280px]"
        >
          <div className="space-y-4">
            <Skeleton className="h-56 rounded-2xl" />
            <Skeleton className="h-40 rounded-2xl" />
          </div>
          <div className="space-y-4">
            <Skeleton className="h-12 rounded-xl" />
            <Skeleton className="h-40 rounded-2xl" />
            <Skeleton className="h-72 rounded-2xl" />
          </div>
          <div className="space-y-4">
            <Skeleton className="h-64 rounded-2xl" />
            <Skeleton className="h-28 rounded-2xl" />
            <Skeleton className="h-48 rounded-2xl" />
          </div>
        </div>
      </ArticlesContainer>
    </div>
  );
}
