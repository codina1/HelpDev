"use client";

import Link from "next/link";
import { motion } from "framer-motion";
import type {
  RoadmapDetailModel,
  RoadmapRelatedItem,
  RoadmapResourceItem,
  RoadmapTimelineStep,
} from "@/data/roadmap-detail";

export function RoadmapInfoSidebar({ model }: { model: RoadmapDetailModel }) {
  const rows = [
    { label: "دسته‌بندی", value: model.category },
    { label: "فناوری اصلی", value: model.tech },
    { label: "سطح", value: model.levelLabel },
    { label: "مدت زمان", value: model.durationLabel },
    { label: "تعداد مراحل", value: `${model.stepsCount.toLocaleString("fa-IR")} مرحله` },
    { label: "تعداد منابع", value: `${model.resourcesCount.toLocaleString("fa-IR")}+` },
    { label: "پروژه‌ها", value: model.projectsCount.toLocaleString("fa-IR") },
    { label: "آخرین بروزرسانی", value: model.updatedAtLabel },
    { label: "سازنده مسیر", value: model.author },
  ];

  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4 shadow-[0_0_28px_rgba(139,92,246,0.1)] backdrop-blur-xl">
      <h2 className="text-[13px] font-extrabold text-white">اطلاعات مسیر</h2>
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

export function RoadmapSocialProof({ model }: { model: RoadmapDetailModel }) {
  return (
    <aside className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4">
      <div className="flex items-center justify-between gap-3">
        <div>
          <p className="text-[12px] text-[#64748B]">امتیاز مسیر</p>
          <p className="mt-1 text-[18px] font-extrabold text-[#FBBF24]">
            ★ {model.rating.toLocaleString("fa-IR")}
          </p>
        </div>
        <div className="text-end">
          <p className="text-[12px] text-[#64748B]">یادگیرنده فعال</p>
          <p className="mt-1 text-[18px] font-extrabold text-white">{model.studentsLabel}</p>
        </div>
      </div>
    </aside>
  );
}

export function RoadmapRelatedList({ items }: { items: RoadmapRelatedItem[] }) {
  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">مسیرهای مرتبط</h2>
      <ul className="mt-3 space-y-2">
        {items.map((item) => (
          <li key={item.id}>
            <Link
              href={`/roadmap/${item.slug}`}
              className="group flex items-center gap-2.5 rounded-xl border border-transparent p-2 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
            >
              <span
                className={`inline-flex h-10 w-10 items-center justify-center rounded-xl bg-gradient-to-br ${item.tone} text-[11px] font-bold text-white`}
              >
                {item.tech.slice(0, 2)}
              </span>
              <span className="min-w-0 flex-1">
                <span className="block truncate text-[12.5px] font-bold text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                  {item.title}
                </span>
                <span className="text-[11px] text-[#64748B]">{item.tech}</span>
              </span>
              <span className="text-[#64748B]">←</span>
            </Link>
          </li>
        ))}
      </ul>
    </aside>
  );
}

export function RoadmapCertificateBanner() {
  return (
    <motion.aside
      whileHover={{ y: -2 }}
      className="overflow-hidden rounded-2xl border border-[rgba(139,92,246,0.4)] bg-[linear-gradient(145deg,rgba(139,92,246,0.35),rgba(37,99,235,0.12)_50%,rgba(11,18,36,0.95))] p-4 shadow-[0_0_32px_rgba(139,92,246,0.25)]"
    >
      <p className="text-[20px]" aria-hidden>
        🏆
      </p>
      <h2 className="mt-1 text-[14px] font-extrabold leading-6 text-white">
        پس از تکمیل مسیر، گواهی پایان مسیر دریافت کنید
      </h2>
      <Link
        href="/roadmap"
        className="mt-3 inline-flex h-9 items-center justify-center rounded-xl bg-white/95 px-3.5 text-[12px] font-bold text-[#4C1D95] no-underline"
      >
        مشاهده نمونه مدرک
      </Link>
    </motion.aside>
  );
}

export function RoadmapStepsNav({
  steps,
  activeId,
}: {
  steps: RoadmapTimelineStep[];
  activeId?: string;
}) {
  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.2)] bg-[#10182D]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">مراحل مسیر</h2>
      <ol className="mt-3 max-h-[280px] space-y-1 overflow-auto [scrollbar-width:thin]">
        {steps.map((step) => (
          <li key={step.id}>
            <a
              href={`#steps`}
              className={[
                "flex items-center gap-2 rounded-lg px-2 py-1.5 text-[12px] transition",
                activeId === step.id || step.completed
                  ? "bg-[#8B5CF6]/12 font-bold text-[#E9D5FF]"
                  : "text-[#94A3B8] hover:bg-white/[0.04] hover:text-white",
              ].join(" ")}
            >
              <span
                className={[
                  "inline-flex h-5 w-5 items-center justify-center rounded-full text-[10px] font-bold",
                  step.completed ? "bg-[#8B5CF6] text-white" : "bg-white/[0.06] text-[#94A3B8]",
                ].join(" ")}
              >
                {step.completed ? "✓" : step.order.toLocaleString("fa-IR")}
              </span>
              <span className="truncate">{step.title}</span>
            </a>
          </li>
        ))}
      </ol>
    </aside>
  );
}

export function RoadmapProgressCard({ model }: { model: RoadmapDetailModel }) {
  const radius = 36;
  const circ = 2 * Math.PI * radius;
  const offset = circ - (model.progressPercent / 100) * circ;

  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.25)] bg-[#10182D]/95 p-4 shadow-[0_0_24px_rgba(139,92,246,0.12)]">
      <h2 className="text-[13px] font-extrabold text-white">پیشرفت شما</h2>
      <div className="mt-3 flex items-center gap-4">
        <div className="relative h-[88px] w-[88px]">
          <svg viewBox="0 0 96 96" className="-rotate-90">
            <circle cx="48" cy="48" r={radius} fill="none" stroke="rgba(255,255,255,0.08)" strokeWidth="8" />
            <circle
              cx="48"
              cy="48"
              r={radius}
              fill="none"
              stroke="#8B5CF6"
              strokeWidth="8"
              strokeLinecap="round"
              strokeDasharray={circ}
              strokeDashoffset={offset}
              className="drop-shadow-[0_0_8px_rgba(139,92,246,0.8)]"
            />
          </svg>
          <span className="absolute inset-0 flex items-center justify-center text-[15px] font-extrabold text-white">
            {model.progressPercent.toLocaleString("fa-IR")}٪
          </span>
        </div>
        <div>
          <p className="text-[13px] font-bold text-[#E5E7EB]">
            {model.completedSteps.toLocaleString("fa-IR")} از {model.stepsCount.toLocaleString("fa-IR")} مرحله
          </p>
          <p className="mt-1 text-[11.5px] text-[#64748B]">ادامه دهید تا مسیر را کامل کنید</p>
        </div>
      </div>
      <Link
        href="#steps"
        className="mt-4 inline-flex h-10 w-full items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] text-[13px] font-bold text-white no-underline shadow-[0_0_16px_rgba(139,92,246,0.35)]"
      >
        ادامه یادگیری
      </Link>
    </aside>
  );
}

export function RoadmapResourcesCard({ resources }: { resources: RoadmapResourceItem[] }) {
  return (
    <aside className="rounded-2xl border border-white/[0.08] bg-[#0B1224]/95 p-4">
      <h2 className="text-[13px] font-extrabold text-white">منابع پیشنهادی</h2>
      <ul className="mt-3 space-y-2">
        {resources.map((item) => (
          <li key={item.id}>
            <a
              href={item.href}
              target="_blank"
              rel="noopener noreferrer"
              className="group flex items-start gap-2.5 rounded-xl border border-transparent p-2 no-underline transition hover:border-white/[0.08] hover:bg-white/[0.03]"
            >
              <span className="inline-flex h-9 w-9 shrink-0 items-center justify-center rounded-lg bg-[#8B5CF6]/15 text-[12px] font-bold text-[#E9D5FF]">
                {item.title.slice(0, 1)}
              </span>
              <span className="min-w-0 flex-1">
                <span className="block text-[12.5px] font-bold text-[#E5E7EB] group-hover:text-[#E9D5FF]">
                  {item.title}
                </span>
                <span className="mt-0.5 block text-[11px] text-[#64748B]">{item.description}</span>
              </span>
              <span className="text-[#64748B]">↗</span>
            </a>
          </li>
        ))}
      </ul>
    </aside>
  );
}

export function RoadmapCommunityCard() {
  return (
    <aside className="rounded-2xl border border-[rgba(139,92,246,0.3)] bg-[linear-gradient(145deg,rgba(139,92,246,0.2),rgba(11,18,36,0.95))] p-4">
      <h2 className="text-[13px] font-extrabold text-white">نیاز به راهنمایی دارید؟</h2>
      <p className="mt-1.5 text-[12px] leading-6 text-[#CBD5E1]">
        سوالات خود را در انجمن توسعه‌دهندگان HelpDev بپرسید.
      </p>
      <Link
        href="/articles"
        className="mt-3 inline-flex h-9 items-center rounded-xl bg-[#8B5CF6] px-3.5 text-[12px] font-bold text-white no-underline"
      >
        مشاهده انجمن
      </Link>
    </aside>
  );
}
