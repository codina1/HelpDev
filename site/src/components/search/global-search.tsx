"use client";

import Link from "next/link";
import { useRouter, useSearchParams } from "next/navigation";
import { useEffect, useMemo, useState, useTransition } from "react";
import {
  SEARCH_TABS,
  countByTab,
  searchKnowledge,
} from "@/lib/search";
import type { SearchTab } from "@/types";

type GlobalSearchProps = {
  initialQuery?: string;
  initialTab?: SearchTab;
};

export function GlobalSearch({
  initialQuery = "",
  initialTab = "news",
}: GlobalSearchProps) {
  const router = useRouter();
  const searchParams = useSearchParams();
  const [isPending, startTransition] = useTransition();

  const queryFromUrl = searchParams.get("q") ?? initialQuery;
  const tabFromUrl = (searchParams.get("tab") as SearchTab | null) ?? initialTab;

  const [query, setQuery] = useState(queryFromUrl);
  const [tab, setTab] = useState<SearchTab>(
    SEARCH_TABS.some((item) => item.id === tabFromUrl) ? tabFromUrl : "news",
  );

  useEffect(() => {
    setQuery(queryFromUrl);
    if (SEARCH_TABS.some((item) => item.id === tabFromUrl)) {
      setTab(tabFromUrl);
    }
  }, [queryFromUrl, tabFromUrl]);

  const counts = useMemo(() => countByTab(query), [query]);
  const results = useMemo(() => searchKnowledge(query, tab), [query, tab]);

  function updateUrl(nextQuery: string, nextTab: SearchTab) {
    const params = new URLSearchParams();
    if (nextQuery.trim()) params.set("q", nextQuery.trim());
    params.set("tab", nextTab);

    startTransition(() => {
      router.replace(`/search?${params.toString()}`, { scroll: false });
    });
  }

  function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
    event.preventDefault();
    updateUrl(query, tab);
  }

  function handleTabChange(nextTab: SearchTab) {
    setTab(nextTab);
    updateUrl(query, nextTab);
  }

  return (
    <div className="space-y-6">
      <section className="ui-panel p-5 sm:p-7">
        <p className="ui-kicker mb-2">موتور دانش</p>
        <h1 className="ui-title text-[1.35rem]">
          جستجو در کتابخانه توسعه‌دهندگان
        </h1>
        <p className="ui-body mt-2 max-w-2xl">
          اخبار، رودمپ، ابزارها و دوره‌ها را از یک نقطه جستجو کنید.
        </p>

        <form onSubmit={handleSubmit} className="mt-6">
          <label htmlFor="global-search" className="sr-only">
            Search
          </label>
          <div className="relative">
            <span className="pointer-events-none absolute inset-y-0 start-3.5 flex items-center text-muted">
              <SearchIcon />
            </span>
            <input
              id="global-search"
              type="search"
              value={query}
              onChange={(event) => setQuery(event.target.value)}
              placeholder="مثلاً React، API، Docker..."
              className="ui-input h-12 pe-28 ps-11 text-sm"
              autoFocus
            />
            <button
              type="submit"
              className="ui-btn ui-btn-primary absolute inset-y-1.5 end-1.5 px-3.5"
            >
              جستجو
            </button>
          </div>
        </form>
      </section>

      <div
        className="flex flex-wrap gap-2"
        role="tablist"
        aria-label="Result categories"
      >
        {SEARCH_TABS.map((item) => {
          const isActive = tab === item.id;

          return (
            <button
              key={item.id}
              type="button"
              role="tab"
              aria-selected={isActive}
              onClick={() => handleTabChange(item.id)}
              className={[
                "ui-chip px-3.5 py-2",
                isActive ? "ui-chip-active" : "",
              ].join(" ")}
            >
              {item.label}
              <span className="ml-2 font-mono text-[11px] opacity-80">
                {counts[item.id]}
              </span>
            </button>
          );
        })}
      </div>

      <div className={isPending ? "opacity-70 transition-opacity" : ""}>
        {results.length > 0 ? (
          <ul className="ui-panel divide-y divide-border/80 overflow-hidden">
            {results.map((result) => (
              <li key={result.id}>
                <Link
                  href={result.href}
                  className="block px-5 py-4 transition-colors duration-200 hover:bg-accent-soft/40 sm:px-6"
                >
                  <div className="mb-2 flex flex-wrap items-center gap-2.5">
                    <span className="ui-badge uppercase tracking-wider">
                      {result.tab}
                    </span>
                    <span className="ui-meta">{result.meta}</span>
                  </div>
                  <h2 className="ui-heading">{result.title}</h2>
                  <p className="ui-body mt-1.5">{result.summary}</p>
                </Link>
              </li>
            ))}
          </ul>
        ) : (
          <div className="ui-panel border-dashed px-4 py-14 text-center">
            <p className="ui-heading">نتیجه‌ای در این بخش نیست</p>
            <p className="ui-body mt-2">
              تب دیگری را امتحان کنید یا عبارتی مثل{" "}
              <button
                type="button"
                onClick={() => {
                  setQuery("react");
                  updateUrl("react", tab);
                }}
                className="font-mono text-accent transition-colors hover:text-foreground"
              >
                react
              </button>
              ,{" "}
              <button
                type="button"
                onClick={() => {
                  setQuery("api");
                  updateUrl("api", tab);
                }}
                className="font-mono text-accent transition-colors hover:text-foreground"
              >
                api
              </button>
              , or{" "}
              <button
                type="button"
                onClick={() => {
                  setQuery("docker");
                  updateUrl("docker", tab);
                }}
                className="font-mono text-accent transition-colors hover:text-foreground"
              >
                docker
              </button>
              {" "}
              را جستجو کنید.
            </p>
          </div>
        )}
      </div>
    </div>
  );
}

function SearchIcon() {
  return (
    <svg
      width="16"
      height="16"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2"
      strokeLinecap="round"
      strokeLinejoin="round"
      aria-hidden
    >
      <circle cx="11" cy="11" r="7" />
      <path d="m20 20-3-3" />
    </svg>
  );
}
