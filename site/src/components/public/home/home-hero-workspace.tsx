import type { ReactNode } from "react";

/**
 * 3D SaaS product workspace — 650×500 on desktop.
 * Visual: AI TL · Prompt TR · Code BL · Tools BR · Laptop center
 */
export function HomeHeroWorkspace() {
  return (
    <div
      className="home-hero-workspace relative mx-auto aspect-[13/10] w-full max-w-[320px] sm:max-w-[450px] lg:max-w-[min(100%,560px)] min-[1440px]:max-w-[620px]"
      role="img"
      aria-label="فضای کار توسعه‌دهنده با لپ‌تاپ و کارت‌های شناور AI، Prompt، Code و Tools"
    >
      {/* Glow layers — stronger purple + blue depth */}
      <div
        className="pointer-events-none absolute inset-[-18%] rounded-full bg-[radial-gradient(circle_at_50%_42%,rgba(124,58,237,0.62),transparent_56%)] blur-[64px]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute -right-[6%] top-[2%] h-[68%] w-[68%] rounded-full bg-[radial-gradient(circle,rgba(37,99,235,0.42),transparent_58%)] blur-[56px]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute -left-[8%] bottom-[8%] h-[45%] w-[45%] rounded-full bg-[radial-gradient(circle,rgba(124,58,237,0.28),transparent_65%)] blur-[48px]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-[8%] rounded-[42%] bg-[radial-gradient(circle_at_center,rgba(15,23,42,0.45),transparent_68%)] blur-2xl"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-x-[12%] bottom-[2%] h-[22%] rounded-[50%] bg-[radial-gradient(ellipse_at_center,rgba(124,58,237,0.5),transparent_70%)] blur-lg"
        aria-hidden
      />

      {/* Laptop — central 3D product */}
      <div className="home-hero-float-slow absolute inset-x-[11%] top-[16%] z-[2] sm:inset-x-[13%] sm:top-[14%] lg:inset-x-[14%] lg:top-[12%]">
        <div
          className="overflow-hidden rounded-t-[18px] border border-white/15 bg-[#0B1224] shadow-[0_48px_110px_rgba(2,6,23,0.88),0_0_80px_rgba(124,58,237,0.45),0_0_56px_rgba(37,99,235,0.22)]"
          style={{ transform: "perspective(1100px) rotateX(8deg) rotateY(-4deg)" }}
        >
          <div className="flex items-center gap-1.5 border-b border-white/[0.07] bg-[#070B16] px-4 py-2.5">
            <span className="h-2 w-2 rounded-full bg-[#F43F5E]/9" />
            <span className="h-2 w-2 rounded-full bg-[#FBBF24]/9" />
            <span className="h-2 w-2 rounded-full bg-[#34D399]/9" />
            <span className="ms-2 truncate font-mono text-[10px] text-[#64748B]">helpdev — workspace</span>
          </div>
          <div className="grid grid-cols-[auto_1fr] gap-0 bg-[#050816] p-4 sm:p-5" dir="ltr">
            <div className="select-none pe-3 font-mono text-[9px] leading-[1.65] text-[#334155] sm:text-[10px]">
              {Array.from({ length: 11 }, (_, i) => (
                <div key={i}>{i + 1}</div>
              ))}
            </div>
            <pre className="overflow-hidden font-mono text-[9px] leading-[1.65] text-[#CBD5E1] sm:text-[10.5px]">
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
          className="relative mx-auto h-3.5 w-[114%] -translate-x-[6%] rounded-b-2xl bg-gradient-to-b from-[#1E293B] via-[#152033] to-[#0B1224] shadow-[0_22px_48px_rgba(2,6,23,0.65)]"
          style={{ transform: "perspective(1100px) rotateX(8deg) rotateY(-4deg)" }}
        >
          <div className="absolute inset-x-[36%] top-1.5 h-1 rounded-full bg-white/20" />
        </div>
      </div>

      {/* Visual Top-Left: AI */}
      <FloatingCard
        className="home-hero-float left-0 top-[2%] z-[3] sm:left-[1%]"
        delay="0s"
        accent="purple"
        label="AI"
        title="AI Assistant"
        icon={<SparkIcon />}
      />

      {/* Visual Top-Right: Prompt */}
      <FloatingCard
        className="home-hero-float right-0 top-[4%] z-[3] sm:right-[1%]"
        delay="0.45s"
        accent="blue"
        label="PROMPT"
        title="Cursor"
        icon={<PromptIcon />}
      />

      {/* Visual Bottom-Left: Code */}
      <FloatingCard
        className="home-hero-float bottom-[1%] left-0 z-[3] sm:left-[1%]"
        delay="1.5s"
        accent="purple"
        label="CODE"
        title="Code"
        icon={<CodeIcon />}
      />

      {/* Visual Bottom-Right: Tools */}
      <FloatingCard
        className="home-hero-float bottom-[2%] right-0 z-[3] sm:right-[1%]"
        delay="1.05s"
        accent="blue"
        label="TOOLS"
        title="JSON · JWT · Regex"
        icon={<ToolsIcon />}
      />
    </div>
  );
}

type FloatingCardProps = {
  className?: string;
  delay: string;
  accent: "purple" | "blue";
  label: string;
  title: string;
  icon: ReactNode;
};

const ACCENT: Record<FloatingCardProps["accent"], string> = {
  purple:
    "border-[rgba(124,58,237,0.55)] text-[#C4B5FD] shadow-[0_22px_50px_rgba(124,58,237,0.4),0_0_36px_rgba(124,58,237,0.28)]",
  blue:
    "border-[rgba(37,99,235,0.52)] text-[#93C5FD] shadow-[0_22px_50px_rgba(37,99,235,0.34),0_0_36px_rgba(37,99,235,0.22)]",
};

function FloatingCard({ className = "", delay, accent, label, title, icon }: FloatingCardProps) {
  return (
    <div
      className={[
        "absolute w-[9.75rem] rounded-[18px] border bg-[rgba(15,23,42,0.8)] p-3.5 backdrop-blur-md sm:w-[11rem] sm:p-4",
        ACCENT[accent],
        className,
      ].join(" ")}
      style={{ animationDelay: delay }}
    >
      <div className="mb-2 flex items-center gap-2">
        <span className="flex h-8 w-8 items-center justify-center rounded-lg bg-white/[0.07]">{icon}</span>
        <span className="font-mono text-[10px] font-bold tracking-wide opacity-90">{label}</span>
      </div>
      <p className="text-[13px] font-bold leading-4 text-white sm:text-[14px]">{title}</p>
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
