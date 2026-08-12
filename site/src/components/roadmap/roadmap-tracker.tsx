"use client";

import { useMemo, useState } from "react";
import type { RoadmapStep } from "@/types";

type RoadmapTrackerProps = {
  title: string;
  description: string;
  steps: RoadmapStep[];
};

export function RoadmapTracker({
  title,
  description,
  steps,
}: RoadmapTrackerProps) {
  const [completed, setCompleted] = useState<Set<string>>(new Set());
  const [focusId, setFocusId] = useState<string | null>(null);

  const progress = useMemo(() => {
    if (steps.length === 0) return 0;
    return Math.round((completed.size / steps.length) * 100);
  }, [completed.size, steps.length]);

  const nextStep = useMemo(
    () => steps.find((step) => !completed.has(step.id)) ?? null,
    [completed, steps],
  );

  function toggleStep(id: string) {
    setCompleted((current) => {
      const next = new Set(current);
      if (next.has(id)) {
        next.delete(id);
      } else {
        next.add(id);
      }
      return next;
    });
  }

  function continueLearning() {
    if (!nextStep) return;

    setFocusId(nextStep.id);
    document
      .getElementById(`step-${nextStep.id}`)
      ?.scrollIntoView({ behavior: "smooth", block: "center" });
  }

  return (
    <div className="space-y-6">
      <div className="ui-panel p-5 sm:p-6">
        <div className="flex flex-wrap items-start justify-between gap-4">
          <div>
            <p className="ui-kicker mb-2">مسیر یادگیری</p>
            <h1 className="ui-title text-[1.35rem]">{title}</h1>
            <p className="ui-body mt-2 max-w-xl">{description}</p>
          </div>
          <button
            type="button"
            onClick={continueLearning}
            disabled={!nextStep}
            className="ui-btn ui-btn-primary px-4 py-2.5"
          >
            {nextStep ? "ادامه یادگیری" : "رودمپ تکمیل شد"}
          </button>
        </div>

        <div className="mt-6">
          <div className="mb-2.5 flex items-center justify-between">
            <span className="ui-meta">
              {completed.size} از {steps.length} تکمیل شده
            </span>
            <span className="text-sm font-semibold text-accent">{progress}%</span>
          </div>
          <div
            className="ui-progress h-2.5"
            role="progressbar"
            aria-valuenow={progress}
            aria-valuemin={0}
            aria-valuemax={100}
            aria-label="پیشرفت رودمپ"
          >
            <div
              className="ui-progress-bar"
              style={{ width: `${progress}%` }}
            />
          </div>
        </div>
      </div>

      <ol className="space-y-3">
        {steps.map((step, index) => {
          const isDone = completed.has(step.id);
          const isFocused = focusId === step.id;
          const isNext = nextStep?.id === step.id;

          return (
            <li key={step.id} id={`step-${step.id}`}>
              <label
                className={[
                  "ui-card flex cursor-pointer gap-4 p-4 sm:p-5",
                  isDone ? "opacity-70" : "",
                  isFocused || isNext
                    ? "border-accent/35 shadow-[0_0_24px_rgba(34,211,238,0.12)]"
                    : "",
                ].join(" ")}
              >
                <input
                  type="checkbox"
                  checked={isDone}
                  onChange={() => toggleStep(step.id)}
                  className="mt-1 h-4 w-4 shrink-0 cursor-pointer accent-[var(--accent)]"
                  aria-label={`علامت‌گذاری ${step.title} به‌عنوان تکمیل‌شده`}
                />
                <span className="min-w-0 flex-1">
                  <span className="flex items-center gap-2">
                    <span className="ui-meta font-semibold">
                      مرحله {index + 1}
                    </span>
                    {isNext && !isDone ? (
                      <span className="ui-badge">بعدی</span>
                    ) : null}
                  </span>
                  <span
                    className={[
                      "mt-1.5 block text-sm font-semibold tracking-tight",
                      isDone
                        ? "text-muted line-through"
                        : "text-foreground",
                    ].join(" ")}
                  >
                    {step.title}
                  </span>
                  <span className="ui-body mt-1 block">{step.description}</span>
                </span>
              </label>
            </li>
          );
        })}
      </ol>
    </div>
  );
}
