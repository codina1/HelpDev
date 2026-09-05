import { ArticlesContainer } from "@/components/articles/articles-container";

function SkeletonBlock({ className }: { className?: string }) {
  return (
    <div
      className={[
        "animate-pulse rounded-xl bg-white/[0.06]",
        className ?? "",
      ].join(" ")}
    />
  );
}

export default function PromptLabDetailLoading() {
  return (
    <div className="bg-[#050816] pb-12 pt-4" dir="rtl" aria-busy="true" aria-live="polite">
      <span className="sr-only">در حال بارگذاری پرامپت</span>
      <ArticlesContainer>
        <SkeletonBlock className="mb-4 h-4 w-64" />
        <SkeletonBlock className="mb-6 h-[220px] w-full rounded-[20px]" />
        <div
          dir="ltr"
          className="grid grid-cols-1 items-start gap-6 xl:grid-cols-[minmax(0,1fr)_280px]"
        >
          <div className="space-y-4">
            <SkeletonBlock className="h-12 w-full rounded-xl" />
            <SkeletonBlock className="h-40 w-full rounded-2xl" />
            <SkeletonBlock className="h-72 w-full rounded-2xl" />
            <div className="grid grid-cols-2 gap-3 lg:grid-cols-4">
              <SkeletonBlock className="h-28 rounded-2xl" />
              <SkeletonBlock className="h-28 rounded-2xl" />
              <SkeletonBlock className="h-28 rounded-2xl" />
              <SkeletonBlock className="h-28 rounded-2xl" />
            </div>
          </div>
          <div className="space-y-4">
            <SkeletonBlock className="h-9 w-36 rounded-xl" />
            <SkeletonBlock className="h-52 rounded-2xl" />
            <SkeletonBlock className="h-28 rounded-2xl" />
            <SkeletonBlock className="h-48 rounded-2xl" />
          </div>
        </div>
      </ArticlesContainer>
    </div>
  );
}
