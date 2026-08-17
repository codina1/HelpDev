import { PromptLabCardSkeleton } from "@/components/public/prompt-lab/prompt-lab-card";

export default function PromptLabDetailLoading() {
  return (
    <div dir="rtl" aria-busy="true" aria-live="polite" style={{ background: "var(--home-bg)" }}>
      <span className="sr-only">در حال بارگذاری پرامپت</span>
      <div
        style={{
          display: "grid",
          gap: "1rem",
          padding: "2rem 1.5rem",
          gridTemplateColumns: "repeat(auto-fit, minmax(14rem, 1fr))",
        }}
      >
        <PromptLabCardSkeleton />
        <PromptLabCardSkeleton />
        <PromptLabCardSkeleton />
      </div>
    </div>
  );
}
