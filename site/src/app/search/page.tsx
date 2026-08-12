import type { Metadata } from "next";
import { Suspense } from "react";
import { ApiGlobalSearch } from "@/components/search/api-global-search";
import { PageLoadingState } from "@/components/ui/page-loading-state";

export const metadata: Metadata = {
  title: "جستجو",
};

export default function SearchPage() {
  return (
    <Suspense
      fallback={
        <div className="mx-auto max-w-3xl px-4 py-10">
          <PageLoadingState />
        </div>
      }
    >
      <ApiGlobalSearch />
    </Suspense>
  );
}
