"use client";

import { useMemo, useState } from "react";
import type { LearningPreferenceDto, LearningProfileDto } from "@/lib/api/learning-personalization";
import { LEARNING_TOPIC_OPTIONS } from "@/lib/api/learning-personalization";
import { SkillSelector } from "@/components/learning/skill-selector";
import { GoalSelector } from "@/components/learning/goal-selector";

type Props = {
  initial: LearningProfileDto | null;
  saving: boolean;
  error: string | null;
  onSave: (payload: {
    experienceLevel: string;
    learningGoals: string;
    currentSkills: string;
    preferredTopics: LearningPreferenceDto[];
  }) => Promise<void>;
};

export function LearningProfileForm({ initial, saving, error, onSave }: Props) {
  const [experienceLevel, setExperienceLevel] = useState(initial?.experienceLevel || "Beginner");
  const [learningGoals, setLearningGoals] = useState(initial?.learningGoals || "");
  const [skills, setSkills] = useState(
    (initial?.currentSkills || "")
      .split(",")
      .map((s) => s.trim())
      .filter(Boolean),
  );
  const [topics, setTopics] = useState<LearningPreferenceDto[]>(
    initial?.preferredTopics?.length
      ? initial.preferredTopics
      : LEARNING_TOPIC_OPTIONS.map((topic, index) => ({
          topic,
          priority: index + 1,
          interestLevel: 3,
        })),
  );

  const selectedTopics = useMemo(() => new Set(topics.map((t) => t.topic)), [topics]);

  function toggleTopic(topic: string) {
    setTopics((prev) => {
      if (prev.some((t) => t.topic === topic)) {
        return prev.filter((t) => t.topic !== topic);
      }
      return [...prev, { topic, priority: prev.length + 1, interestLevel: 3 }];
    });
  }

  function updateInterest(topic: string, interestLevel: number) {
    setTopics((prev) =>
      prev.map((t) => (t.topic === topic ? { ...t, interestLevel } : t)),
    );
  }

  return (
    <form
      dir="rtl"
      className="space-y-6"
      onSubmit={(e) => {
        e.preventDefault();
        void onSave({
          experienceLevel,
          learningGoals,
          currentSkills: skills.join(", "),
          preferredTopics: topics,
        });
      }}
    >
      <div className="space-y-2">
        <label className="text-sm text-slate-300">سطح تجربه</label>
        <select
          value={experienceLevel}
          onChange={(e) => setExperienceLevel(e.target.value)}
          className="w-full rounded-xl border border-white/10 bg-[#12141f] px-3 py-2.5 text-sm text-white"
        >
          <option value="Beginner">مبتدی</option>
          <option value="Intermediate">متوسط</option>
          <option value="Advanced">پیشرفته</option>
        </select>
      </div>

      <GoalSelector value={learningGoals} onChange={setLearningGoals} />
      <SkillSelector skills={skills} onChange={setSkills} />

      <div className="space-y-3">
        <p className="text-sm text-slate-300">موضوعات مورد علاقه</p>
        <div className="flex flex-wrap gap-2">
          {LEARNING_TOPIC_OPTIONS.map((topic) => {
            const active = selectedTopics.has(topic);
            return (
              <button
                key={topic}
                type="button"
                onClick={() => toggleTopic(topic)}
                className={`rounded-full px-3 py-1.5 text-xs font-semibold transition ${
                  active
                    ? "bg-emerald-500/20 text-emerald-200 ring-1 ring-emerald-400/40"
                    : "bg-white/5 text-slate-300 ring-1 ring-white/10"
                }`}
              >
                {topic}
              </button>
            );
          })}
        </div>
        {topics.map((topic) => (
          <label key={topic.topic} className="flex items-center justify-between gap-3 text-sm text-slate-300">
            <span>{topic.topic}</span>
            <input
              type="range"
              min={1}
              max={5}
              value={topic.interestLevel}
              onChange={(e) => updateInterest(topic.topic, Number(e.target.value))}
              className="w-40"
            />
          </label>
        ))}
      </div>

      {error ? <p className="text-sm text-rose-300">{error}</p> : null}

      <button
        type="submit"
        disabled={saving}
        className="rounded-xl bg-emerald-600 px-5 py-2.5 text-sm font-bold text-white hover:bg-emerald-500 disabled:opacity-60"
      >
        {saving ? "در حال ذخیره..." : "ذخیره پروفایل یادگیری"}
      </button>
    </form>
  );
}
