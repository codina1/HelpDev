import Link from "next/link";

type OutlineLinkButtonProps = {
  href: string;
  children: React.ReactNode;
  className?: string;
};

export function OutlineLinkButton({
  href,
  children,
  className = "",
}: OutlineLinkButtonProps) {
  return (
    <Link
      href={href}
      className={[
        "group relative mt-3 block w-full overflow-hidden rounded-xl border border-violet-500/35 bg-violet-500/[0.04] py-2.5 text-center text-[12px] font-bold text-violet-300 transition-all duration-300",
        "hover:-translate-y-0.5 hover:border-violet-400/70 hover:bg-violet-500/[0.12] hover:text-white",
        "hover:shadow-[0_0_0_1px_rgba(167,139,250,0.25),0_8px_28px_rgba(139,92,246,0.22)]",
        "active:translate-y-0 active:shadow-none",
        "focus-ring",
        className,
      ].join(" ")}
    >
      <span
        aria-hidden
        className="pointer-events-none absolute inset-0 -translate-x-full bg-gradient-to-l from-transparent via-white/10 to-transparent transition-transform duration-700 group-hover:translate-x-full"
      />
      <span
        aria-hidden
        className="pointer-events-none absolute inset-0 bg-gradient-to-l from-violet-600/0 via-indigo-500/0 to-violet-600/0 opacity-0 transition-opacity duration-300 group-hover:from-violet-600/15 group-hover:via-indigo-500/10 group-hover:to-violet-600/15 group-hover:opacity-100"
      />
      <span className="relative z-10 inline-flex items-center justify-center gap-1.5">
        {children}
        <ChevronIcon />
      </span>
    </Link>
  );
}

function ChevronIcon() {
  return (
    <svg
      width="12"
      height="12"
      viewBox="0 0 24 24"
      fill="none"
      stroke="currentColor"
      strokeWidth="2.5"
      className="transition-transform duration-300 group-hover:-translate-x-1"
      aria-hidden
    >
      <path d="m15 18-6-6 6-6" />
    </svg>
  );
}
