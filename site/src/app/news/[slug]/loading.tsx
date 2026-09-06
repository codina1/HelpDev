import { ArticlesContainer } from "@/components/articles/articles-container";

function Skeleton({ className }: { className?: string }) {
  return <div className={["animate-pulse rounded-xl bg-white/[0.06]", className ?? ""].join(" ")} />;
}

export default function NewsDetailLoading() {
  return (
    <div className="bg-[#050816] pb-12 pt-4" dir="rtl" aria-busy="true">
      <span className="sr-only">در حال بارگذاری خبر</span>
      <ArticlesContainer>
        <Skeleton className="mb-5 h-4 w-72" />
        <Skeleton className="mb-6 h-[220px] w-full rounded-[22px]" />
        <div className="space-y-4">
          <Skeleton className="h-8 w-2/3" />
          <Skeleton className="h-40 w-full rounded-2xl" />
          <Skeleton className="h-40 w-full rounded-2xl" />
        </div>
      </ArticlesContainer>
    </div>
  );
}
