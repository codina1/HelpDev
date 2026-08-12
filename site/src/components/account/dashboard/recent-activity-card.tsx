import { MOCK_ACTIVITY } from "@/data/account-dashboard";

export function RecentActivityCard() {
  return (
    <section className="dash-card p-5">
      <h2 className="text-[15px] font-bold text-white">فعالیت‌های اخیر</h2>

      <ul className="mt-4 space-y-4">
        {MOCK_ACTIVITY.map((item, index) => (
          <li key={item.id} className="relative flex gap-3 ps-1">
            {index < MOCK_ACTIVITY.length - 1 && (
              <span className="absolute start-[18px] top-9 h-[calc(100%+4px)] w-px bg-white/10" />
            )}
            <span
              className={`relative z-10 flex h-9 w-9 shrink-0 items-center justify-center rounded-full text-sm ${item.color}`}
            >
              {item.icon}
            </span>
            <div className="min-w-0 pb-1">
              <p className="text-[13px] leading-6 text-slate-200">{item.text}</p>
              <p className="text-[11px] text-slate-500">{item.time}</p>
            </div>
          </li>
        ))}
      </ul>
    </section>
  );
}
