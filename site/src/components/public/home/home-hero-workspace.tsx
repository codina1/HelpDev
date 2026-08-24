import type { ReactNode } from "react";

/**
 * Floating developer workspace visual for the homepage hero.
 * Pure decorative composition — Laptop + editor + floating AI/Prompt/Tools/Code cards.
 */
export function HomeHeroWorkspace() {
  return (
    <div
      className="home-hero-workspace relative mx-auto aspect-[4/3] w-full max-w-[min(100%,22rem)] sm:max-w-[28rem] lg:max-w-[32rem]"
      role="img"
      aria-label="فضای کار توسعه‌دهنده با لپ‌تاپ، ویرایشگر کد و کارت‌های AI، Prompt، Tools و Code"
    >
      {/* Glow / blur atmosphere */}
      <div
        className="pointer-events-none absolute inset-[-12%] rounded-full bg-[radial-gradient(circle_at_center,rgba(124,58,237,0.45),transparent_62%)] blur-2xl"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-[8%] rounded-full bg-[radial-gradient(circle_at_60%_40%,rgba(6,182,212,0.22),transparent_55%)] blur-xl"
        aria-hidden
      />

      {/* Platform disc */}
      <div
        className="pointer-events-none absolute inset-x-[12%] bottom-[6%] h-[18%] rounded-[50%] bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.35),transparent_70%)] blur-md"
        aria-hidden
      />

      {/* Laptop */}
      <div className="home-hero-float-slow absolute inset-x-[14%] top-[18%] z-[2]">
        <div className="overflow-hidden rounded-t-xl border border-white/10 bg-[#0B1224] shadow-[0_24px_60px_rgba(2,6,23,0.65),0_0_40px_rgba(124,58,237,0.25)]">
          {/* Title bar */}
          <div className="flex items-center gap-1.5 border-b border-white/[0.06] bg-[#070B16] px-3 py-2">
            <span className="h-1.5 w-1.5 rounded-full bg-[#F43F5E]/80" />
            <span className="h-1.5 w-1.5 rounded-full bg-[#FBBF24]/80" />
            <span className="h-1.5 w-1.5 rounded-full bg-[#34D399]/80" />
            <span className="ms-2 truncate font-mono text-[9px] text-[#94A3B8]">helpdev — workspace</span>
          </div>
          {/* Code editor */}
          <div className="grid grid-cols-[auto_1fr] gap-0 bg-[#050816] p-2.5 sm:p-3" dir="ltr">
            <div className="select-none pe-2 font-mono text-[8px] leading-[1.55] text-[#475569] sm:text-[9px]">
              {Array.from({ length: 8 }, (_, i) => (
                <div key={i}>{i + 1}</div>
              ))}
            </div>
            <pre className="overflow-hidden font-mono text-[8px] leading-[1.55] text-[#CBD5E1] sm:text-[9px]">
              <code>
                <span className="text-[#C084FC]">async</span>{" "}
                <span className="text-[#67E8F9]">function</span>{" "}
                <span className="text-[#A5B4FC]">build</span>() {"{"}
                {"\n"}
                {"  "}
                <span className="text-[#94A3B8]">{"// HelpDev AI workspace"}</span>
                {"\n"}
                {"  "}
                <span className="text-[#C084FC]">const</span> prompt ={" "}
                <span className="text-[#86EFAC]">&quot;ship faster&quot;</span>;
                {"\n"}
                {"  "}
                <span className="text-[#C084FC]">await</span> agent.run(prompt);
                {"\n"}
                {"  "}
                <span className="text-[#C084FC]">return</span>{" "}
                <span className="text-[#67E8F9]">tools</span>.map(t =&gt; t.id);
                {"\n"}
                {"}"}
                {"\n"}
                {"\n"}
                <span className="text-[#94A3B8]">build();</span>
              </code>
            </pre>
          </div>
        </div>
        {/* Laptop base */}
        <div className="relative mx-auto h-2 w-[108%] -translate-x-[3.5%] rounded-b-md bg-gradient-to-b from-[#1E293B] to-[#0F172A] shadow-lg">
          <div className="absolute inset-x-[38%] top-0 h-0.5 rounded-full bg-white/10" />
        </div>
      </div>

      {/* Floating cards */}
      <FloatingCard
        className="home-hero-float start-[2%] top-[8%] z-[3]"
        delay="0s"
        accent="purple"
        label="AI"
        title="AI Assistant"
        body="پاسخ زمینه‌دار از دانش HelpDev"
        icon={<SparkIcon />}
      />
      <FloatingCard
        className="home-hero-float end-[0%] top-[14%] z-[3]"
        delay="0.7s"
        accent="cyan"
        label="PROMPT"
        title="Prompt Card"
        body="قالب‌های آماده برای Cursor و Claude"
        icon={<PromptIcon />}
      />
      <FloatingCard
        className="home-hero-float start-[0%] bottom-[10%] z-[3]"
        delay="1.2s"
        accent="blue"
        label="TOOLS"
        title="Tools Card"
        body="JSON · JWT · Regex · SQL"
        icon={<ToolsIcon />}
      />
      <FloatingCard
        className="home-hero-float end-[2%] bottom-[6%] z-[3]"
        delay="1.8s"
        accent="green"
        label="CODE"
        title="Code Card"
        body="snippet · review · refactor"
        icon={<CodeIcon />}
      />
    </div>
  );
}

type FloatingCardProps = {
  className?: string;
  delay: string;
  accent: "purple" | "cyan" | "blue" | "green";
  label: string;
  title: string;
  body: string;
  icon: ReactNode;
};

const ACCENT: Record<FloatingCardProps["accent"], string> = {
  purple: "from-[#7C3AED]/30 to-transparent border-[#7C3AED]/35 text-[#C4B5FD]",
  cyan: "from-[#06B6D4]/25 to-transparent border-[#06B6D4]/35 text-[#67E8F9]",
  blue: "from-[#6366F1]/25 to-transparent border-[#6366F1]/35 text-[#A5B4FC]",
  green: "from-[#34D399]/20 to-transparent border-[#34D399]/30 text-[#6EE7B7]",
};

function FloatingCard({ className = "", delay, accent, label, title, body, icon }: FloatingCardProps) {
  return (
    <div
      className={[
        "absolute w-[7.5rem] rounded-xl border bg-[#0B1224]/90 p-2.5 shadow-[0_12px_32px_rgba(2,6,23,0.55)] backdrop-blur-md sm:w-[8.75rem] sm:p-3",
        `bg-gradient-to-bl ${ACCENT[accent]}`,
        className,
      ].join(" ")}
      style={{ animationDelay: delay }}
    >
      <div className="mb-1.5 flex items-center gap-1.5">
        <span className="flex h-6 w-6 items-center justify-center rounded-md bg-white/[0.06]">{icon}</span>
        <span className="font-mono text-[9px] font-bold tracking-wide opacity-90">{label}</span>
      </div>
      <p className="text-[11px] font-bold leading-4 text-white sm:text-[12px]">{title}</p>
      <p className="mt-0.5 line-clamp-2 text-[9px] leading-snug text-[#94A3B8] sm:text-[10px]">{body}</p>
    </div>
  );
}

function SparkIcon() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2.5 13.8 8.2 19.5 10 13.8 11.8 12 17.5 10.2 11.8 4.5 10 10.2 8.2 12 2.5Z" />
    </svg>
  );
}

function PromptIcon() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M4 6h16M4 12h10M4 18h14" />
    </svg>
  );
}

function ToolsIcon() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
    </svg>
  );
}

function CodeIcon() {
  return (
    <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="m8 8-4 4 4 4M16 8l4 4-4 4M13 6l-2 12" />
    </svg>
  );
}
