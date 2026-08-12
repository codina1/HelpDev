import { MOCK_STATS, WEEKLY_ACTIVITY } from "@/data/account-dashboard";

const METRICS = [
  { label: "مقالات خوانده‌شده", value: MOCK_STATS.readArticles, trend: MOCK_STATS.trends.readArticles },
  { label: "زمان مطالعه", value: `${MOCK_STATS.studyHours} ساعت`, trend: MOCK_STATS.trends.studyHours },
  { label: "مقالات ذخیره‌شده", value: MOCK_STATS.savedNotes, trend: MOCK_STATS.trends.savedArticles },
  { label: "ابزار استفاده‌شده", value: MOCK_STATS.usedTools, trend: MOCK_STATS.trends.usedTools },
] as const;

export function AnalyticsSection() {
  const maxValue = Math.max(...WEEKLY_ACTIVITY.map((item) => item.value));

  return (
    <section className="dash-card p-5 sm:p-6">
      <div className="grid gap-6 lg:grid-cols-[1fr_1.2fr]">
        <div className="grid grid-cols-2 gap-3">
          {METRICS.map((metric) => (
            <div
              key={metric.label}
              className="rounded-xl border border-white/[0.06] bg-white/[0.02] p-4"
            >
              <p className="text-[11px] text-slate-500">{metric.label}</p>
              <p className="mt-2 text-2xl font-black text-white">{metric.value}</p>
              <p className="mt-1 text-[11px] font-bold text-emerald-400">
                ↑ {metric.trend}%
              </p>
            </div>
          ))}
        </div>

        <div>
          <h3 className="text-[14px] font-bold text-white">فعالیت هفتگی</h3>
          <div className="mt-5 flex h-40 items-end justify-between gap-2">
            {WEEKLY_ACTIVITY.map((item) => (
              <div key={item.day} className="flex flex-1 flex-col items-center gap-2">
                <div
                  className="w-full rounded-t-lg bg-gradient-to-t from-violet-600/80 to-indigo-400/60 transition-all hover:from-violet-500 hover:to-indigo-300"
                  style={{ height: `${(item.value / maxValue) * 100}%`, minHeight: "8px" }}
                />
                <span className="text-[10px] font-semibold text-slate-500">{item.day}</span>
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
