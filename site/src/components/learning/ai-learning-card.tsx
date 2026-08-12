"use client";

type Props = {
  title: string;
  children: React.ReactNode;
  action?: React.ReactNode;
};

export function AiLearningCard({ title, children, action }: Props) {
  return (
    <section
      dir="rtl"
      className="rounded-2xl border border-white/10 bg-gradient-to-br from-[#12141f] to-[#0d1018] p-5 shadow-[0_0_40px_rgba(16,185,129,0.08)]"
    >
      <div className="mb-4 flex items-center justify-between gap-3">
        <h2 className="text-base font-bold text-white">{title}</h2>
        {action}
      </div>
      {children}
    </section>
  );
}
