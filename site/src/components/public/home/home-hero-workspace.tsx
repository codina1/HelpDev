import type { ReactNode } from "react";

/**
 * Floating AI developer workspace — laptop center, cards around it.
 * Card placement (visual): Prompt TR · Tools BR · AI TL · Code BL
 */
export function HomeHeroWorkspace() {
  return (
    <div
      className="home-hero-workspace relative mx-auto aspect-[4/3] w-full max-w-[min(100%,20rem)] sm:max-w-[26rem] lg:max-w-[30rem]"
      role="img"
      aria-label="فضای کار توسعه‌دهنده با لپ‌تاپ، ویرایشگر کد و کارت‌های AI، Prompt، Tools و Code"
    >
      <div
        className="pointer-events-none absolute inset-[-18%] rounded-full bg-[radial-gradient(circle_at_center,rgba(124,58,237,0.5),transparent_60%)] blur-3xl"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-[5%] rounded-full bg-[radial-gradient(circle_at_70%_35%,rgba(6,182,212,0.28),transparent_55%)] blur-2xl"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-x-[10%] bottom-[4%] h-[22%] rounded-[50%] bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.4),transparent_70%)] blur-md"
        aria-hidden
      />

      {/* Laptop — center */}
      <div className="home-hero-float-slow absolute inset-x-[16%] top-[20%] z-[2]">
        <div className="overflow-hidden rounded-t-xl border border-white/12 bg-[#0B1224] shadow-[0_28px_70px_rgba(2,6,23,0.7),0_0_48px_rgba(124,58,237,0.3)]">
          <div className="flex items-center gap-1.5 border-b border-white/[0.06] bg-[#070B16] px-3 py-2">
            <span className="h-1.5 w-1.5 rounded-full bg-[#F43F5E]/85" />
            <span className="h-1.5 w-1.5 rounded-full bg-[#FBBF24]/85" />
            <span className="h-1.5 w-1.5 rounded-full bg-[#34D399]/85" />
            <span className="ms-2 truncate font-mono text-[9px] text-[#64748B]">helpdev — workspace</span>
          </div>
          <div className="grid grid-cols-[auto_1fr] gap-0 bg-[#050816] p-2.5 sm:p-3" dir="ltr">
            <div className="select-none pe-2 font-mono text-[8px] leading-[1.55] text-[#334155] sm:text-[9px]">
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
        <div className="relative mx-auto h-2.5 w-[110%] -translate-x-[4.5%] rounded-b-lg bg-gradient-to-b from-[#1E293B] to-[#0F172A] shadow-lg">
          <div className="absolute inset-x-[40%] top-0.5 h-0.5 rounded-full bg-white/15" />
        </div>
      </div>

      {/* Top-right (RTL start): Prompt */}
      <FloatingCard
        className="home-hero-float start-[-2%] top-[6%] z-[3] sm:start-0"
        delay="0.4s"
        accent="cyan"
        label="PROMPT"
        title="Prompt Card"
        body="قالب‌های Cursor و Claude"
        icon={<PromptIcon />}
      />

      {/* Bottom-right (RTL start): Tools */}
      <FloatingCard
        className="home-hero-float start-[-4%] bottom-[4%] z-[3] sm:start-0"
        delay="1.1s"
        accent="blue"
        label="TOOLS"
        title="Tools Card"
        body="JSON · JWT · Regex"
        icon={<ToolsIcon />}
      />

      {/* Top-left (RTL end): AI */}
      <FloatingCard
        className="home-hero-float end-[-2%] top-[4%] z-[3] sm:end-0"
        delay="0s"
        accent="purple"
        label="AI"
        title="AI Assistant"
        body="پاسخ زمینه‌دار HelpDev"
        icon={<SparkIcon />}
      />

      {/* Bottom-left (RTL end): Code */}
      <FloatingCard
        className="home-hero-float end-[-4%] bottom-[2%] z-[3] sm:end-0"
        delay="1.6s"
        accent="green"
        label="CODE"
        title="Code Card"
        body="snippet · refactor"
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
  purple: "border-[#7C3AED]/40 text-[#C4B5FD] shadow-[0_12px_32px_rgba(124,58,237,0.25)]",
  cyan: "border-[#06B6D4]/40 text-[#67E8F9] shadow-[0_12px_32px_rgba(6,182,212,0.2)]",
  blue: "border-[#6366F1]/40 text-[#A5B4FC] shadow-[0_12px_32px_rgba(99,102,241,0.2)]",
  green: "border-[#34D399]/35 text-[#6EE7B7] shadow-[0_12px_32px_rgba(52,211,153,0.15)]",
};

function FloatingCard({ className = "", delay, accent, label, title, body, icon }: FloatingCardProps) {
  return (
    <div
      className={[
        "absolute w-[7.25rem] rounded-xl border bg-[#0B1224]/92 p-2.5 backdrop-blur-md sm:w-[8.5rem] sm:p-3",
        ACCENT[accent],
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
