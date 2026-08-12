import Link from "next/link";
import { Badge } from "@/components/ui/public/badge";
import { Section } from "@/components/ui/public/section";

export function AiAssistantCta() {
  return (
    <Section aria-labelledby="ai-cta-title">
      <div className="relative overflow-hidden rounded-3xl border border-[color:var(--border-strong)] bg-gradient-to-l from-indigo-950/80 via-[color:var(--surface)] to-violet-950/70 p-6 sm:p-8 lg:p-10">
        <div
          className="pointer-events-none absolute -start-10 top-0 h-40 w-40 rounded-full bg-[color:var(--accent)]/20 blur-3xl"
          aria-hidden
        />
        <Badge variant="ai" className="mb-3">
          AI Assistant
        </Badge>
        <h2 id="ai-cta-title" className="text-xl font-extrabold text-white sm:text-2xl lg:text-3xl">
          دستیار یادگیری هوشمند HelpDev
        </h2>
        <p className="mt-3 max-w-2xl text-sm leading-7 text-slate-300 sm:text-[15px]">
          مسیر یادگیری شخصی‌سازی‌شده، پیشنهاد محتوا و پاسخ grounded روی دانش پلتفرم — همین حالا شروع کنید.
        </p>
        <div className="mt-6 flex flex-wrap gap-3">
          <Link
            href="/learning/assistant"
            className="focus-ring inline-flex items-center rounded-xl bg-gradient-to-l from-[color:var(--accent)] to-[color:var(--accent-2)] px-5 py-2.5 text-[13px] font-bold text-white shadow-[0_8px_28px_color-mix(in_srgb,var(--accent)_35%,transparent)] transition hover:-translate-y-0.5"
          >
            باز کردن دستیار
          </Link>
          <Link
            href="/learning"
            className="focus-ring inline-flex items-center rounded-xl border border-white/15 bg-white/5 px-5 py-2.5 text-[13px] font-semibold text-slate-200 hover:bg-white/10"
          >
            هاب یادگیری
          </Link>
        </div>
      </div>
    </Section>
  );
}
