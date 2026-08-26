/**
 * 3D SaaS product workspace — reference illustration.
 */
export function HomeHeroWorkspace() {
  return (
    <div
      className="home-hero-workspace relative mx-auto aspect-[13/10] w-full max-w-[320px] sm:max-w-[450px] lg:max-w-[min(100%,560px)] min-[1440px]:max-w-[620px]"
      role="img"
      aria-label="فضای کار توسعه‌دهنده با لپ‌تاپ و کارت‌های شناور AI، Prompt، Code و Tools"
    >
      <div
        className="pointer-events-none absolute inset-[-12%] rounded-full bg-[radial-gradient(circle_at_50%_55%,rgba(124,58,237,0.45),transparent_58%)] blur-[48px]"
        aria-hidden
      />
      <div
        className="pointer-events-none absolute inset-x-[10%] bottom-[0%] h-[28%] rounded-[50%] bg-[radial-gradient(ellipse_at_center,rgba(37,99,235,0.35),transparent_70%)] blur-xl"
        aria-hidden
      />
      <img
        src="/home/hero-workspace.webp"
        alt=""
        width={1024}
        height={815}
        decoding="async"
        fetchPriority="high"
        className="home-hero-float-slow relative z-[1] h-full w-full object-contain drop-shadow-[0_28px_60px_rgba(2,6,23,0.75)]"
      />
    </div>
  );
}
