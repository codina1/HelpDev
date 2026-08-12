"use client";

import { useState } from "react";

type Props = {
  skills: string[];
  onChange: (skills: string[]) => void;
};

export function SkillSelector({ skills, onChange }: Props) {
  const [draft, setDraft] = useState("");

  function addSkill() {
    const next = draft.trim();
    if (!next || skills.includes(next)) {
      setDraft("");
      return;
    }
    onChange([...skills, next]);
    setDraft("");
  }

  return (
    <div className="space-y-2" dir="rtl">
      <label className="text-sm text-slate-300">مهارت‌های فعلی</label>
      <div className="flex gap-2">
        <input
          value={draft}
          onChange={(e) => setDraft(e.target.value)}
          placeholder="مثلاً C#، React"
          className="flex-1 rounded-xl border border-white/10 bg-[#12141f] px-3 py-2.5 text-sm text-white"
          onKeyDown={(e) => {
            if (e.key === "Enter") {
              e.preventDefault();
              addSkill();
            }
          }}
        />
        <button
          type="button"
          onClick={addSkill}
          className="rounded-xl bg-white/10 px-3 py-2 text-sm text-white"
        >
          افزودن
        </button>
      </div>
      <div className="flex flex-wrap gap-2">
        {skills.map((skill) => (
          <button
            key={skill}
            type="button"
            onClick={() => onChange(skills.filter((s) => s !== skill))}
            className="rounded-full bg-sky-500/15 px-3 py-1 text-xs text-sky-200"
          >
            {skill} ×
          </button>
        ))}
      </div>
    </div>
  );
}
