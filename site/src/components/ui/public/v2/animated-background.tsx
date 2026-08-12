type AnimatedBackgroundProps = {
  className?: string;
  variant?: "hero" | "page";
};

/**
 * Decorative ambient background — no data, no content claims.
 */
export function AnimatedBackground({ className = "", variant = "page" }: AnimatedBackgroundProps) {
  return (
    <div className={["pointer-events-none absolute inset-0 overflow-hidden", className].join(" ")} aria-hidden>
      <div
        className={[
          "pub-glow-pulse absolute -top-24 start-1/2 h-[420px] w-[720px] -translate-x-1/2 rounded-full blur-3xl",
          "bg-[radial-gradient(circle,color-mix(in_srgb,var(--pub-primary)_28%,transparent),transparent_70%)]",
        ].join(" ")}
      />
      {variant === "hero" ? (
        <>
          <div className="absolute -end-20 top-20 h-64 w-64 rounded-full bg-[color:var(--pub-secondary)]/10 blur-3xl" />
          <div className="absolute bottom-0 start-0 h-48 w-48 rounded-full bg-[color:var(--pub-primary-2)]/15 blur-3xl" />
          <svg className="absolute inset-0 h-full w-full opacity-[0.12]" xmlns="http://www.w3.org/2000/svg">
            <defs>
              <pattern id="pub-grid" width="36" height="36" patternUnits="userSpaceOnUse">
                <path d="M 36 0 L 0 0 0 36" fill="none" stroke="white" strokeWidth="0.5" />
              </pattern>
            </defs>
            <rect width="100%" height="100%" fill="url(#pub-grid)" />
          </svg>
        </>
      ) : null}
    </div>
  );
}
