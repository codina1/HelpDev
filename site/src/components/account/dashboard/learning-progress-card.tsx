import Link from "next/link";
import { MOCK_LEARNING } from "@/data/account-dashboard";

export function LearningProgressCard() {
  return (
    <section className="dash-card p-5">
      <div className="flex items-center justify-between gap-3">
        <h2 className="text-[15px] font-bold text-white">ادامه مسیر یادگیری</h2>
        <span className="text-[12px] font-bold text-violet-300">{MOCK_LEARNING.progress}%</span>
      </div>

      <p className="mt-3 text-[14px] font-semibold text-slate-200">{MOCK_LEARNING.title}</p>

      <div className="mt-4 h-2 overflow-hidden rounded-full bg-white/10">
        <div
          className="h-full rounded-full bg-gradient-to-l from-violet-500 to-indigo-500 transition-all"
          style={{ width: `${MOCK_LEARNING.progress}%` }}
        />
      </div>

      <p className="mt-3 text-[12px] text-slate-400">
        گام بعدی: <span className="text-slate-200">{MOCK_LEARNING.nextChapter}</span>
      </p>

      <Link
        href={MOCK_LEARNING.href}
        className="focus-ring mt-4 inline-flex rounded-xl bg-gradient-to-l from-violet-600 to-indigo-600 px-4 py-2.5 text-[12px] font-bold text-white"
      >
        ادامه یادگیری
      </Link>
    </section>
  );
}
