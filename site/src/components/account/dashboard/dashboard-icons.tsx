type IconProps = {
  className?: string;
  size?: number;
};

export function HelpDevLogo({ className = "", size = 22 }: IconProps) {
  return (
    <svg
      width={size}
      height={size}
      viewBox="0 0 24 24"
      fill="none"
      className={className}
      aria-hidden
    >
      <path
        d="M12 2L20 7V17L12 22L4 17V7L12 2Z"
        fill="url(#hd-logo)"
        stroke="rgba(255,255,255,0.2)"
        strokeWidth="0.5"
      />
      <path
        d="M12 7L16 9.5V14.5L12 17L8 14.5V9.5L12 7Z"
        fill="white"
        fillOpacity="0.9"
      />
      <defs>
        <linearGradient id="hd-logo" x1="4" y1="2" x2="20" y2="22">
          <stop stopColor="#8b5cf6" />
          <stop offset="1" stopColor="#6366f1" />
        </linearGradient>
      </defs>
    </svg>
  );
}

export function NavIcon({
  name,
  className = "",
  size = 18,
}: IconProps & { name: string }) {
  const props = {
    width: size,
    height: size,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.75,
    strokeLinecap: "round" as const,
    strokeLinejoin: "round" as const,
    className,
    "aria-hidden": true,
  };

  switch (name) {
    case "home":
      return (
        <svg {...props}>
          <path d="M3 10.5 12 3l9 7.5V20a1 1 0 0 1-1 1h-5v-6H9v6H4a1 1 0 0 1-1-1z" />
        </svg>
      );
    case "news":
      return (
        <svg {...props}>
          <path d="M4 5h16v14H4z" />
          <path d="M8 9h8M8 13h5" />
        </svg>
      );
    case "roadmap":
      return (
        <svg {...props}>
          <path d="M4 6h4l2 12h4l2-12h4" />
        </svg>
      );
    case "tools":
      return (
        <svg {...props}>
          <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L4 17l3 3 5.3-5.3a4 4 0 0 0 5.4-5.4z" />
        </svg>
      );
    case "prompt":
      return (
        <svg {...props}>
          <path d="M12 3l1.5 4.5L18 9l-4.5 1.5L12 15l-1.5-4.5L6 9l4.5-1.5z" />
        </svg>
      );
    case "courses":
      return (
        <svg {...props}>
          <path d="M4 7.5 12 4l8 3.5v9L12 20l-8-3.5z" />
          <path d="M12 11v9" />
        </svg>
      );
    case "user":
      return (
        <svg {...props}>
          <circle cx="12" cy="8" r="3.5" />
          <path d="M5 20c0-3.5 3.1-6 7-6s7 2.5 7 6" />
        </svg>
      );
    case "search":
      return (
        <svg {...props}>
          <circle cx="11" cy="11" r="6.5" />
          <path d="m20 20-3.5-3.5" />
        </svg>
      );
    case "bell":
      return (
        <svg {...props}>
          <path d="M18 8a6 6 0 1 0-12 0c0 6-2 8-2 8h16s-2-2-2-8" />
          <path d="M10 20a2 2 0 0 0 4 0" />
        </svg>
      );
    case "chevron":
      return (
        <svg {...props}>
          <path d="m6 9 6 6 6-6" />
        </svg>
      );
    case "menu":
      return (
        <svg {...props}>
          <path d="M4 7h16M4 12h16M4 17h16" />
        </svg>
      );
    case "crown":
      return (
        <svg {...props} fill="currentColor" stroke="none">
          <path d="M4 18h16v2H4zm2-8 3 4 3-6 3 6 3-4 2 8H6z" />
        </svg>
      );
    case "pencil":
      return (
        <svg {...props} width={size} height={size}>
          <path d="M12 20h9M16.5 3.5a2.1 2.1 0 0 1 3 3L7 19l-4 1 1-4z" />
        </svg>
      );
    default:
      return null;
  }
}
