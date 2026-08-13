import type { CSSProperties } from "react";
import { PublicSection } from "@/components/ui/public/v2/public-section";
import { HomeWorkflowInput } from "@/components/public/home/home-workflow-input";
import {
  HomeWorkflowStep,
  type HomeWorkflowStepItem,
} from "@/components/public/home/home-workflow-step";

export const HOME_WORKFLOW_STEPS: readonly HomeWorkflowStepItem[] = [
  {
    id: "understand",
    number: 1,
    title: "درک مسئله",
    caption: "صورت‌مسئله، محدودیت و هدف را شفاف کنید",
  },
  {
    id: "analyze",
    number: 2,
    title: "تحلیل معماری",
    caption: "گزینه‌ها، مرزها و الگوهای سیستم را بسنجید",
  },
  {
    id: "decide",
    number: 3,
    title: "تصمیم فناوری",
    caption: "استک و ابزار مناسب را انتخاب کنید",
  },
  {
    id: "plan",
    number: 4,
    title: "نقشه اجرا",
    caption: "گام‌های اولویت‌دار و قابل‌اجرا بسازید",
  },
  {
    id: "solve",
    number: 5,
    title: "راه‌حل مهندسی",
    caption: "به دانش، ابزار و مسیر اجرا برسید",
  },
];

export const HOME_WORKFLOW_QUESTION = "چطور معماری یک سیستم SaaS را طراحی کنم؟";

const ACTIVE_INDEX = 2;

function stepState(index: number): "upcoming" | "active" | "done" {
  if (index === ACTIVE_INDEX) return "active";
  if (index < ACTIVE_INDEX) return "done";
  return "upcoming";
}

/**
 * Homepage workflow — question mockup + five-step CSS timeline.
 */
export function HomeWorkflowSection() {
  const progress = ACTIVE_INDEX / (HOME_WORKFLOW_STEPS.length - 1);
  const visualStyle = {
    "--home-workflow-progress": `${progress * 100}%`,
  } as CSSProperties;

  return (
    <PublicSection
      className="home-workflow home-reveal"
      containerSize="wide"
      aria-labelledby="home-workflow-heading"
    >
      <div className="mx-auto max-w-3xl text-center">
        <h2
          id="home-workflow-heading"
          className="font-extrabold tracking-tight text-[color:var(--home-text)]"
          style={{
            fontSize: "clamp(1.35rem, 2.4vw, var(--home-title-size))",
            lineHeight: 1.45,
          }}
        >
          از سؤال تا راهکار با هوش HelpDev
        </h2>
        <p
          className="mx-auto mt-3 max-w-xl text-[color:var(--home-text-muted)]"
          style={{
            fontSize: "var(--home-body-size)",
            lineHeight: "var(--home-body-leading)",
          }}
        >
          سؤال مهندسی را بپرسید؛ هوش HelpDev آن را به مسیر تحلیل، تصمیم و اجرا تبدیل می‌کند.
        </p>
      </div>

      <div className="home-workflow-panel mt-8 sm:mt-10" style={visualStyle}>
        <HomeWorkflowInput question={HOME_WORKFLOW_QUESTION} />

        <div className="home-workflow-visual">
          <div className="home-workflow-line" aria-hidden>
            <span className="home-workflow-line-fill" />
          </div>
          <ol className="home-workflow-steps" aria-label="گردش‌کار پنج‌مرحله‌ای مهندسی">
            {HOME_WORKFLOW_STEPS.map((item, index) => (
              <HomeWorkflowStep key={item.id} item={item} state={stepState(index)} />
            ))}
          </ol>
        </div>
      </div>
    </PublicSection>
  );
}
