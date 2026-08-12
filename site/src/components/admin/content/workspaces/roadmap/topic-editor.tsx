"use client";

import type { UpsertRoadmapTopicItemDto } from "@/lib/api/content";

type TopicEditorProps = {
  topics: UpsertRoadmapTopicItemDto[];
  onChange: (topics: UpsertRoadmapTopicItemDto[]) => void;
  disabled?: boolean;
};

export function TopicEditor({ topics, onChange, disabled }: TopicEditorProps) {
  const add = () => {
    onChange([...topics, { title: "", description: null, order: topics.length }]);
  };

  const update = (index: number, patch: Partial<UpsertRoadmapTopicItemDto>) => {
    onChange(topics.map((t, i) => (i === index ? { ...t, ...patch } : t)));
  };

  const remove = (index: number) => {
    onChange(topics.filter((_, i) => i !== index).map((t, order) => ({ ...t, order })));
  };

  return (
    <div className="space-y-2 rounded-lg border border-[var(--adm-border)] p-3">
      <div className="flex items-center justify-between">
        <h4 className="adm-text text-[12px] font-bold">موضوعات</h4>
        <button
          type="button"
          className="adm-btn adm-btn-ghost adm-focus text-[11px]"
          disabled={disabled}
          onClick={add}
        >
          افزودن موضوع
        </button>
      </div>
      {topics.length === 0 ? (
        <p className="adm-muted text-[11px]">موضوعی ثبت نشده.</p>
      ) : (
        <ul className="space-y-2">
          {topics.map((topic, index) => (
            <li key={`${topic.id ?? "new"}-${index}`} className="flex gap-2">
              <input
                className="adm-input flex-1"
                placeholder={`موضوع ${index + 1}`}
                value={topic.title}
                disabled={disabled}
                onChange={(e) => update(index, { title: e.target.value })}
              />
              <button
                type="button"
                className="adm-btn adm-btn-outline adm-focus text-[11px]"
                disabled={disabled}
                onClick={() => remove(index)}
              >
                حذف
              </button>
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}
