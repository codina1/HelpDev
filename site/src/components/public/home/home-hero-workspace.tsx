import type { ReactNode } from "react";

/**
 * Large floating AI developer workspace (≈600×500 on desktop).
 * Laptop center · Prompt TR · Tools BR · AI TL · Code BL
 */
export function HomeHeroWorkspace() {
  return (
    <div
      className="home-hero-workspace relative mx-auto h-[340px] w-full max-w-[360px] sm:h-[420px] sm:max-w-[480px] lg:h-[500px] lg:w-[600px] lg:max-w-none"
      role="img"
      aria-label="فضای کار توسعه‌دهنده با لپ‌تاپ، ویرایشگر کد و کارت‌های AI، Prompt، Tools و Code"
    >
      <div
        className="pointer-events-none absolute inset-[-8%] rounded-full bg-[radial-gradient(circle_at_center,rgba(124,58,237,0.45),transparent_58%)] blur-3xl"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-[8%] rounded-full bg-[radial-gradient(circle_at_70%_30%,rgba(6,182,212,0.28),transparent_55%)] blur-2xl"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-x-[12%] bottom-[2%] h-[20%] rounded-[50%] bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.35),transparent_70%)] blur-md"
        aria-hidden
      />

      {/* Laptop — larger 3D-ish center piece */}
      <div className="home-hero-float-slow absolute inset-x-[12%] top-[14%] z-[2] sm:inset-x-[14%] sm:top-[12%] lg:inset-x-[15%] lg:top-[10%]">
        <div
          className="overflow-hidden rounded-t-2xl border border-white/12 bg-[#0B1224] shadow-[0_32px_80px_rgba(2,6,23,0.75),0_0_56px_rgba(124,58,237,0.32)]"
          style={{ transform: "perspective(900px) rotateX(6deg)" }}
        >
          <div className="flex items-center gap-1.5 border-b border-white/[0.06] bg-[#070B16] px-3.5 py-2.5">
            <span className="h-2 w-2 rounded-full bg-[#F43F5E]/85" />
            <span className="h-2 w-2 rounded-full bg-[#FBBF24]/85" />
            <span className="h-2 w-2 rounded-full bg-[#34D399]/85" />
            <span className="ms-2 truncate font-mono text-[10px] text-[#64748B]">helpdev — workspace</span>
          </div>
          <div className="grid grid-cols-[auto_1fr] gap-0 bg-[#050816] p-3.5 sm:p-4" dir="ltr">
            <div className="select-none pe-3 font-mono text-[9px] leading-[1.6] text-[#334155] sm:text-[10px]">
              {Array.from({ length: 10 }, (_, i) => (
                <div key={i}>{i + 1}</div>
              ))}
            </div>
            <pre className="overflow-hidden font-mono text-[9px] leading-[1.6] text-[#CBD5E1] sm:text-[10px]">
              <code>
                <span className="text-[#C084FC]">async</span>{" "}
                <span className="text-[#67E8F9]">function</span>{" "}
                <span className="text-[#A5B4FC]">build</span>() {"{"}
                {"\n"}
                {"  "}
                <span className="text-[#64748B]">{"// HelpDev AI workspace"}</span>
                {"\n"}
                {"  "}
                <span className="text-[#C084FC]">const</span> prompt ={" "}
                <span className="text-[#86EFAC]">&quot;ship faster&quot;</span>;
                {"\n"}
                {"  "}
                <span className="text-[#C084FC]">await</span> agent.run(prompt);
                {"\n"}
                {"  "}
                <span className="text-[#C084FC]">const</span> tools = [json, jwt, regex];
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
        <div
          className="relative mx-auto h-3 w-[112%] -translate-x-[5%] rounded-b-xl bg-gradient-to-b from-[#1E293B] to-[#0F172A] shadow-[0_18px_40px_rgba(2,6,23,0.55)]"
          style={{ transform: "perspective(900px) rotateX(6deg)" }}
        >
          <div className="absolute inset-x-[38%] top-1 h-1 rounded-full bg-white/15" />
        </div>
      </div>

      <FloatingCard
        className="home-hero-float start-0 top-[4%] z-[3] sm:start-[2%]"
        delay="0.4s"
        accent="cyan"
        label="PROMPT"
        title="Cursor"
        icon={<PromptIcon />}
      />

      <FloatingCard
        className="home-hero-float start-0 bottom-[2%] z-[3] sm:start-[1%]"
        delay="1.1s"
        accent="cyan"
        label="TOOLS"
        title="JSON · JWT · Regex"
        icon={<ToolsIcon />}
      />

      <FloatingCard
        className="home-hero-float end-0 top-[2%] z-[3] sm:end-[2%]"
        delay="0s"
        accent="purple"
        label="AI"
        title="AI Assistant"
        icon={<SparkIcon />}
      />

      <FloatingCard
        className="home-hero-float end-0 bottom-[0%] z-[3] sm:end-[1%]"
        delay="1.6s"
        accent="purple"
        label="CODE"
        title="Code"
        icon={<CodeIcon />}
      />
    </div>
  );
}

type FloatingCardProps = {
  className?: string;
  delay: string;
  accent: "purple" | "cyan";
  label: string;
  title: string;
  icon: ReactNode;
};

const ACCENT: Record<FloatingCardProps["accent"], string> = {
  purple:
    "border-[rgba(124,58,237,0.45)] text-[#C4B5FD] shadow-[0_12px_36px_rgba(124,58,237,0.28),0_0_24px_rgba(124,58,237,0.18)]",
  cyan:
    "border-[rgba(6,182,212,0.4)] text-[#67E8F9] shadow-[0_12px_36px_rgba(6,182,212,0.22),0_0_24px_rgba(6,182,212,0.14)]",
};

function FloatingCard({ className = "", delay, accent, label, title, icon }: FloatingCardProps) {
  return (
    <div
      className={[
        "absolute w-[8.25rem] rounded-[18px] border bg-[rgba(15,23,42,0.8)] p-3 backdrop-blur-md sm:w-[9.5rem] sm:p-3.5",
        ACCENT[accent],
        className,
      ].join(" ")}
      style={{ animationDelay: delay }}
    >
      <div className="mb-1.5 flex items-center gap-1.5">
        <span className="flex h-7 w-7 items-center justify-center rounded-lg bg-white/[0.06]">{icon}</span>
        <span className="font-mono text-[9px] font-bold tracking-wide opacity-90">{label}</span>
      </div>
      <p className="text-[12px] font-bold leading-4 text-white sm:text-[13px]">{title}</p>
    </div>
  );
}

function SparkIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="currentColor" aria-hidden>
      <path d="M12 2.5 13.8 8.2 19.5 10 13.8 11.8 12 17.5 10.2 11.8 4.5 10 10.2 8.2 12 2.5Z" />
    </svg>
  );
}

function PromptIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M4 6h16M4 12h10M4 18h14" />
    </svg>
  );
}

function ToolsIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L3 18l3 3 6.3-6.3a4 4 0 0 0 5.4-5.4l-3 3-2-2 3-3Z" />
    </svg>
  );
}

function CodeIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <path d="m8 8-4 4 4 4M16 8l4 4-4 4M13 6l-2 12" />
    </svg>
  );
}
