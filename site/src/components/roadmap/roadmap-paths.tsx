"use client";

import { useMemo, useState } from "react";
import Link from "next/link";
import { RoadmapContainer } from "@/components/roadmap/roadmap-container";
import {
  filterRoadmapStages,
  ROADMAP_PATH_FILTERS,
  ROADMAP_STAGES,
  type RoadmapPathFilter,
  type RoadmapStage,
} from "@/data/roadmap-paths";

/** Static class map so Tailwind keeps the column utilities in the build. */
const LG_COLUMNS: Record<number, string> = {
  1: "lg:grid-cols-1",
  2: "lg:grid-cols-2",
  3: "lg:grid-cols-3",
  4: "lg:grid-cols-4",
  5: "lg:grid-cols-5",
  6: "lg:grid-cols-6",
};

const FILTER_ICON_COLOR: Record<string, string> = {
  all: "#FFFFFF",
  frontend: "#22D3EE",
  backend: "#818CF8",
  devops: "#38BDF8",
  mobile: "#A5B4FC",
  ai: "#A78BFA",
  other: "#C084FC",
};

function FilterIcon({ name, color }: { name: string; color: string }) {
  const cls = "h-4 w-4 shrink-0";
  switch (name) {
    case "frontend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.2" y="4.2" width="17.6" height="12.8" rx="2" stroke={color} strokeWidth="1.55" />
          <path d="M8.5 20h7M12 17v3" stroke={color} strokeWidth="1.55" strokeLinecap="round" />
        </svg>
      );
    case "backend":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.8" y="4.2" width="16.4" height="6" rx="1.6" stroke={color} strokeWidth="1.55" />
          <rect x="3.8" y="13.8" width="16.4" height="6" rx="1.6" stroke={color} strokeWidth="1.55" />
          <circle cx="7.4" cy="7.2" r="0.9" fill={color} />
          <circle cx="7.4" cy="16.8" r="0.9" fill={color} />
        </svg>
      );
    case "devops":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.4 8.6c1.8-2.3 5-2.3 6.8 0 1.4 1.8 1.4 4.2 0 6-1.8 2.3-5 2.3-6.8 0-1.4-1.8-1.4-4.2 0-6Z" stroke={color} strokeWidth="1.6" strokeLinejoin="round" />
          <path d="M15.2 8.6c1.5-1.9 4-1.9 5.2.2M8.8 15.4c-1.5 1.9-4 1.9-5.2-.2" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "mobile":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="7" y="2.8" width="10" height="18.4" rx="2.4" stroke={color} strokeWidth="1.6" />
          <path d="M10.6 18.4h2.8" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "ai":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="12" cy="12" r="3.2" stroke={color} strokeWidth="1.6" />
          <path d="M12 3.4v3.4M12 17.2v3.4M3.4 12h3.4M17.2 12h3.4M6 6l2.4 2.4M15.6 15.6 18 18M18 6l-2.4 2.4M8.4 15.6 6 18" stroke={color} strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    case "other":
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <circle cx="5.6" cy="12" r="1.6" fill={color} />
          <circle cx="12" cy="12" r="1.6" fill={color} />
          <circle cx="18.4" cy="12" r="1.6" fill={color} />
        </svg>
      );
    default:
      return (
        <svg className={cls} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.5" y="3.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
          <rect x="13.5" y="3.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
          <rect x="3.5" y="13.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
          <rect x="13.5" y="13.5" width="7" height="7" rx="1.5" stroke={color} strokeWidth="1.6" />
        </svg>
      );
  }
}

function StageIcon({ name, className }: { name: RoadmapStage["icon"]; className?: string }) {
  switch (name) {
    case "layout":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3" y="4" width="18" height="16" rx="2.4" stroke="currentColor" strokeWidth="1.6" />
          <path d="M3 9h18M9 9v11" stroke="currentColor" strokeWidth="1.6" />
        </svg>
      );
    case "server":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <rect x="3.4" y="4.2" width="17.2" height="6.2" rx="1.8" stroke="currentColor" strokeWidth="1.6" />
          <rect x="3.4" y="13.6" width="17.2" height="6.2" rx="1.8" stroke="currentColor" strokeWidth="1.6" />
          <circle cx="7.2" cy="7.3" r="1" fill="currentColor" />
          <circle cx="7.2" cy="16.7" r="1" fill="currentColor" />
        </svg>
      );
    case "infinity":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M8.4 8.4c2 0 2.6 1.6 3.6 3.6 1 2 1.6 3.6 3.6 3.6a3.6 3.6 0 1 0 0-7.2c-2 0-2.6 1.6-3.6 3.6-1 2-1.6 3.6-3.6 3.6a3.6 3.6 0 1 1 0-7.2Z" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    case "rocket":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M13.6 3.6c3.4.6 6.2 3.4 6.8 6.8L12 18.8 5.2 12l8.4-8.4Z" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
          <circle cx="14.6" cy="9.4" r="1.9" stroke="currentColor" strokeWidth="1.6" />
          <path d="M7.6 16.4c-1.6 1.2-2 3.2-2 4.4 1.2 0 3.2-.4 4.4-2" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
        </svg>
      );
    case "trophy":
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M7 4h10v5.2a5 5 0 0 1-10 0V4Z" stroke="currentColor" strokeWidth="1.6" strokeLinejoin="round" />
          <path d="M7 5.6H4.4v1.6A3.2 3.2 0 0 0 7 10.3M17 5.6h2.6v1.6A3.2 3.2 0 0 1 17 10.3M12 14.3V18M8.6 20h6.8" stroke="currentColor" strokeWidth="1.6" strokeLinecap="round" />
        </svg>
      );
    default:
      return (
        <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
          <path d="M9 7.2 4.2 12 9 16.8M15 7.2 19.8 12 15 16.8" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
        </svg>
      );
  }
}

function BookIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M5 6.5A2.5 2.5 0 0 1 7.5 4H19v14.5H7.5A2.5 2.5 0 0 0 5 21V6.5Z" stroke="currentColor" strokeWidth="1.7" strokeLinejoin="round" />
    </svg>
  );
}

function ClockIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <circle cx="12" cy="12" r="8" stroke="currentColor" strokeWidth="1.8" />
      <path d="M12 8v4.2l2.5 1.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" />
    </svg>
  );
}

function ArrowIcon({ className }: { className?: string }) {
  return (
    <svg className={className} viewBox="0 0 24 24" fill="none" aria-hidden>
      <path d="M14 6.5 8.5 12l5.5 5.5" stroke="currentColor" strokeWidth="1.8" strokeLinecap="round" strokeLinejoin="round" />
    </svg>
  );
}

/** Learning paths: filter strip · horizontal timeline · six stage cards · CTA. */
export function RoadmapPaths() {
  const [track, setTrack] = useState<RoadmapPathFilter>("همه");

  const stages = useMemo(() => filterRoadmapStages(ROADMAP_STAGES, track), [track]);
  const activeStep = useMemo(() => {
    const inProgress = stages.filter((stage) => stage.progress > 0);
    return inProgress.length > 0 ? inProgress[0].id : stages[0]?.id;
  }, [stages]);

  return (
    <section id="roadmap-paths" className="bg-[#030713] pb-8 pt-6" dir="rtl">
      <RoadmapContainer>
        <div className="mb-5 flex flex-wrap items-center justify-between gap-4">
          <h2 className="text-[18px] font-extrabold text-white sm:text-[20px]">مسیرهای یادگیری</h2>

          <div className="max-w-full overflow-x-auto [-ms-overflow-style:none] [scrollbar-width:none] [&::-webkit-scrollbar]:hidden">
            <div className="flex w-max flex-nowrap items-center gap-2.5" role="toolbar" aria-label="فیلتر مسیرها">
              {ROADMAP_PATH_FILTERS.map((item) => {
                const isActive = track === item.id;
                return (
                  <button
                    key={item.id}
                    type="button"
                    aria-pressed={isActive}
                    onClick={() => setTrack(item.id)}
                    className={[
                      "inline-flex h-[38px] shrink-0 items-center gap-2 rounded-full border px-4 text-[12.5px] font-semibold transition duration-200",
                      isActive
                        ? "border-transparent bg-gradient-to-r from-[#7C3AED] to-[#6D28D9] text-white shadow-[0_0_14px_rgba(124,58,237,0.32)]"
                        : "border-white/[0.1] bg-[#0F1626]/90 text-[#E5E7EB] hover:border-[rgba(168,85,247,0.4)] hover:text-white",
                    ].join(" ")}
                  >
                    <span>{item.label}</span>
                    <FilterIcon
                      name={item.icon}
                      color={isActive ? "#FFFFFF" : FILTER_ICON_COLOR[item.icon] ?? "#A78BFA"}
                    />
                  </button>
                );
              })}
            </div>
          </div>
        </div>

        {stages.length > 0 ? (
          <>
            <div className="relative hidden pb-4 lg:block" aria-hidden>
              <span className="absolute inset-x-0 top-[9px] h-px bg-gradient-to-l from-transparent via-white/[0.12] to-transparent" />
              <div
                className="relative grid gap-5"
                style={{ gridTemplateColumns: `repeat(${stages.length}, minmax(0, 1fr))` }}
              >
                {stages.map((stage) => {
                  const isActive = stage.id === activeStep;
                  return (
                    <span key={stage.id} className="flex justify-center">
                      <span
                        className={[
                          "block h-[18px] w-[18px] rounded-full border-2 transition",
                          isActive
                            ? "border-[#A855F7] bg-[#7C3AED] shadow-[0_0_12px_rgba(168,85,247,0.6)]"
                            : "border-[#1E3A5F] bg-[#0B1120]",
                        ].join(" ")}
                      />
                    </span>
                  );
                })}
              </div>
            </div>

            <div className={["grid grid-cols-1 gap-5 sm:grid-cols-2", LG_COLUMNS[stages.length] ?? "lg:grid-cols-6"].join(" ")}>
              {stages.map((stage) => (
                <StageCard key={stage.id} stage={stage} active={stage.id === activeStep} />
              ))}
            </div>

            <div className="mt-8 flex justify-center">
              <Link
                href="#roadmap-guide"
                className="focus-ring inline-flex h-11 items-center justify-center gap-2 rounded-xl border border-[rgba(168,85,247,0.45)] bg-[rgba(124,58,237,0.08)] px-6 text-[13px] font-bold text-[#E9D5FF] no-underline shadow-[0_0_16px_rgba(124,58,237,0.18)] transition hover:bg-[rgba(124,58,237,0.16)] hover:text-white"
              >
                مشاهده جزئیات همه مسیرها
                <ArrowIcon className="h-4 w-4 shrink-0" />
              </Link>
            </div>
          </>
        ) : (
          <div className="rounded-[16px] border border-dashed border-white/[0.12] px-4 py-12 text-center text-[13px] text-[#94A3B8]">
            مسیری برای این فیلتر پیدا نشد.
          </div>
        )}
      </RoadmapContainer>
    </section>
  );
}

function StageCard({ stage, active }: { stage: RoadmapStage; active: boolean }) {
  return (
    <article
      className={[
        "flex h-full min-w-0 flex-col items-center rounded-[16px] border p-5 text-center transition duration-200",
        active
          ? "border-[rgba(168,85,247,0.45)] bg-[#0D1020] shadow-[0_0_22px_rgba(124,58,237,0.16)]"
          : "border-white/[0.07] bg-[#080D1E] hover:border-[rgba(168,85,247,0.28)]",
      ].join(" ")}
    >
      <span
        className={[
          "inline-flex h-14 w-14 items-center justify-center rounded-2xl border",
          active
            ? "border-[rgba(168,85,247,0.4)] bg-[rgba(124,58,237,0.14)] text-[#C084FC]"
            : "border-white/[0.08] bg-white/[0.03] text-[#A78BFA]",
        ].join(" ")}
        aria-hidden
      >
        <StageIcon name={stage.icon} className="h-7 w-7" />
      </span>

      <span className="mt-4 inline-flex items-center rounded-md border border-white/[0.08] bg-white/[0.04] px-2.5 py-[3px] text-[10.5px] font-bold text-[#CBD5E1]">
        {stage.stepLabel}
      </span>

      <h3 className="mt-3 text-[14px] font-extrabold leading-6 text-white">{stage.title}</h3>
      <p className="mt-1.5 text-[11.5px] leading-5 text-[#8B98AC]">{stage.description}</p>

      <div className="mt-auto w-full pt-4">
        <div className="flex items-center justify-end text-[10.5px] font-bold text-[#94A3B8]">
          <span>{stage.progress}%</span>
        </div>
        <div className="mt-1.5 h-1.5 w-full overflow-hidden rounded-full bg-white/[0.07]">
          <span
            className="block h-full rounded-full bg-gradient-to-l from-[#7C3AED] to-[#A855F7]"
            style={{ width: `${stage.progress}%` }}
          />
        </div>

        <div className="mt-3 flex items-center justify-between gap-2 border-t border-white/[0.06] pt-3 text-[10.5px] font-semibold text-[#64748B]">
          <span className="inline-flex items-center gap-1 whitespace-nowrap">
            <ClockIcon className="h-3.5 w-3.5 shrink-0 text-[#7C3AED]" />
            <bdi>{stage.duration}</bdi>
          </span>
          <span className="inline-flex items-center gap-1 whitespace-nowrap">
            <BookIcon className="h-3.5 w-3.5 shrink-0 text-[#7C3AED]" />
            <bdi>{stage.lessons}</bdi>
          </span>
        </div>
      </div>
    </article>
  );
}
