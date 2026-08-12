"use client";

const GOAL_SUGGESTIONS = [
  "Become AI Developer",
  "Master ASP.NET Core",
  "Frontend Engineer",
  "DevOps Practitioner",
];

type Props = {
  value: string;
  onChange: (value: string) => void;
};

export function GoalSelector({ value, onChange }: Props) {
  return (
    <div className="space-y-2" dir="rtl">
      <label className="text-sm text-slate-300">هدف یادگیری</label>
      <textarea
        value={value}
        onChange={(e) => onChange(e.target.value)}
        rows={3}
        placeholder="مثلاً Become AI Developer"
        className="w-full rounded-xl border border-white/10 bg-[#12141f] px-3 py-2.5 text-sm text-white"
      />
      <div className="flex flex-wrap gap-2">
        {GOAL_SUGGESTIONS.map((goal) => (
          <button
            key={goal}
            type="button"
            onClick={() => onChange(goal)}
            className="rounded-full bg-violet-500/15 px-3 py-1 text-xs text-violet-200"
          >
            {goal}
          </button>
        ))}
      </div>
    </div>
  );
}
