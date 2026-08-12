import type { AdminIconName } from "@/lib/admin/navigation";

type AdminIconProps = {
  name: AdminIconName;
  className?: string;
  size?: number;
};

/**
 * Single, tree-shakable Admin icon component. Icons are inline stroke SVGs that
 * inherit `currentColor`, so they adapt to light/dark themes automatically. No
 * external icon dependency is introduced.
 */
export function AdminIcon({ name, className, size = 18 }: AdminIconProps) {
  const common = {
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
    focusable: false as const,
  };

  return <svg {...common}>{ICON_PATHS[name]}</svg>;
}

const ICON_PATHS: Record<AdminIconName, React.ReactNode> = {
  dashboard: (
    <>
      <rect x="3" y="3" width="8" height="8" rx="1.5" />
      <rect x="13" y="3" width="8" height="5" rx="1.5" />
      <rect x="13" y="11" width="8" height="10" rx="1.5" />
      <rect x="3" y="14" width="8" height="7" rx="1.5" />
    </>
  ),
  content: (
    <>
      <rect x="4" y="3" width="16" height="18" rx="2" />
      <path d="M8 8h8M8 12h8M8 16h5" />
    </>
  ),
  plus: <path d="M12 5v14M5 12h14" />,
  calendar: (
    <>
      <rect x="3" y="4.5" width="18" height="16" rx="2" />
      <path d="M3 9h18M8 3v3M16 3v3" />
    </>
  ),
  folder: <path d="M3 7a2 2 0 0 1 2-2h4l2 2h6a2 2 0 0 1 2 2v8a2 2 0 0 1-2 2H5a2 2 0 0 1-2-2z" />,
  tag: (
    <>
      <path d="M4 4h7l9 9-7 7-9-9z" />
      <circle cx="8" cy="8" r="1.4" />
    </>
  ),
  media: (
    <>
      <rect x="3" y="4" width="18" height="16" rx="2" />
      <circle cx="9" cy="9.5" r="1.8" />
      <path d="m4 18 5-5 4 4 3-3 4 4" />
    </>
  ),
  seo: (
    <>
      <circle cx="11" cy="11" r="6.5" />
      <path d="m20 20-3.5-3.5" />
      <path d="M9 11h4" />
    </>
  ),
  learning: (
    <>
      <path d="M4 7.5 12 4l8 3.5-8 3.5z" />
      <path d="M6 9.5V15c0 1.2 2.7 2.5 6 2.5s6-1.3 6-2.5V9.5" />
    </>
  ),
  lessons: (
    <>
      <path d="M4 5h7a2 2 0 0 1 2 2v12a2 2 0 0 0-2-2H4z" />
      <path d="M20 5h-7a2 2 0 0 0-2 2v12a2 2 0 0 1 2-2h7z" />
    </>
  ),
  enrollments: (
    <>
      <circle cx="9" cy="8" r="3" />
      <path d="M3 20c0-3.3 2.7-6 6-6 1.3 0 2.5.4 3.5 1.1" />
      <path d="M16 11l2 2 4-4" />
    </>
  ),
  progress: (
    <>
      <path d="M4 19V5" />
      <path d="M4 15l4-4 4 3 8-8" />
    </>
  ),
  toolbox: <path d="M14.7 6.3a4 4 0 0 0-5.4 5.4L4 17l3 3 5.3-5.3a4 4 0 0 0 5.4-5.4z" />,
  runs: (
    <>
      <circle cx="12" cy="12" r="8.5" />
      <path d="m10 8.5 5 3.5-5 3.5z" />
    </>
  ),
  prompt: <path d="M12 3l1.6 4.8L18 9l-4.4 1.2L12 15l-1.6-4.8L6 9l4.4-1.2z" />,
  versions: (
    <>
      <circle cx="6" cy="6" r="2.5" />
      <circle cx="6" cy="18" r="2.5" />
      <circle cx="18" cy="12" r="2.5" />
      <path d="M6 8.5v7M8.4 6.6c5 0 4.4 5.4 7.2 5.4M8.4 17.4c5 0 4.4-5.4 7.2-5.4" />
    </>
  ),
  users: (
    <>
      <circle cx="9" cy="8" r="3.2" />
      <path d="M3 20c0-3.4 2.7-6 6-6s6 2.6 6 6" />
      <path d="M16 5.2a3.2 3.2 0 0 1 0 6M21 20c0-2.6-1.3-4.7-3.5-5.6" />
    </>
  ),
  roles: (
    <>
      <circle cx="12" cy="8" r="3.2" />
      <path d="M6 20c0-3.4 2.7-6 6-6s6 2.6 6 6" />
      <path d="m12 2 1 1.6L15 4l-1 1.6L15 7l-2-.4L12 8l-1-1.4L9 7l1-1.4L9 4l2 .4z" opacity="0" />
    </>
  ),
  shield: <path d="M12 3l7 3v5c0 4.5-3 8.2-7 10-4-1.8-7-5.5-7-10V6z" />,
  activity: <path d="M3 12h4l2.5 7 5-15L17 12h4" />,
  analytics: (
    <>
      <path d="M4 20V4" />
      <rect x="7" y="12" width="3" height="6" rx="0.6" />
      <rect x="12" y="8" width="3" height="10" rx="0.6" />
      <rect x="17" y="5" width="3" height="13" rx="0.6" />
    </>
  ),
  search: (
    <>
      <circle cx="11" cy="11" r="6.5" />
      <path d="m20 20-3.5-3.5" />
    </>
  ),
  bell: (
    <>
      <path d="M18 8a6 6 0 1 0-12 0c0 6-2 8-2 8h16s-2-2-2-8" />
      <path d="M10 20a2 2 0 0 0 4 0" />
    </>
  ),
  flag: (
    <>
      <path d="M5 21V4" />
      <path d="M5 5h11l-2 3 2 3H5" />
    </>
  ),
  settings: (
    <>
      <circle cx="12" cy="12" r="3" />
      <path d="M19.4 13.5a1.7 1.7 0 0 0 .3 1.9l.1.1a2 2 0 1 1-2.8 2.8l-.1-.1a1.7 1.7 0 0 0-2.9 1.2V21a2 2 0 1 1-4 0v-.1a1.7 1.7 0 0 0-2.9-1.2l-.1.1a2 2 0 1 1-2.8-2.8l.1-.1a1.7 1.7 0 0 0-1.2-2.9H3a2 2 0 1 1 0-4h.1a1.7 1.7 0 0 0 1.2-2.9l-.1-.1A2 2 0 1 1 7 4.2l.1.1a1.7 1.7 0 0 0 1.9.3H9a1.7 1.7 0 0 0 1-1.5V3a2 2 0 1 1 4 0v.1a1.7 1.7 0 0 0 2.9 1.2l.1-.1a2 2 0 1 1 2.8 2.8l-.1.1a1.7 1.7 0 0 0-.3 1.9V9a1.7 1.7 0 0 0 1.5 1H21a2 2 0 1 1 0 4h-.1a1.7 1.7 0 0 0-1.5 1z" />
    </>
  ),
  audit: (
    <>
      <path d="M6 3h9l4 4v14H6z" />
      <path d="M14 3v4h4" />
      <path d="M9 12l1.5 1.5L13 11M9 16h5" />
    </>
  ),
  outbox: (
    <>
      <path d="M4 13v6a1 1 0 0 0 1 1h14a1 1 0 0 0 1-1v-6" />
      <path d="M4 13h5l1.5 2h3L15 13h5" />
      <path d="M12 3v7m0-7-3 3m3-3 3 3" />
    </>
  ),
  health: <path d="M3 12h4l2-5 3 10 2.5-7 1.5 2H21" />,
  version: (
    <>
      <path d="M12 3l8 4.5v9L12 21l-8-4.5v-9z" />
      <path d="M12 12v9M12 12l8-4.5M12 12 4 7.5" />
    </>
  ),
  news: (
    <>
      <rect x="4" y="5" width="16" height="14" rx="2" />
      <path d="M8 9h6M8 12h8M8 15h5" />
    </>
  ),
  announcement: (
    <>
      <path d="M4 10v4l10 4V6z" />
      <path d="M14 8a4 4 0 0 1 0 8" />
    </>
  ),
  sun: (
    <>
      <circle cx="12" cy="12" r="4" />
      <path d="M12 2v2M12 20v2M2 12h2M20 12h2M4.9 4.9l1.4 1.4M17.7 17.7l1.4 1.4M19.1 4.9l-1.4 1.4M6.3 17.7l-1.4 1.4" />
    </>
  ),
  moon: <path d="M20 14.5A8 8 0 0 1 9.5 4a7 7 0 1 0 10.5 10.5z" />,
  logout: (
    <>
      <path d="M15 4h3a2 2 0 0 1 2 2v12a2 2 0 0 1-2 2h-3" />
      <path d="M10 17l-5-5 5-5M5 12h11" />
    </>
  ),
  chevron: <path d="m6 9 6 6 6-6" />,
  check: <path d="M5 12.5 10 17l9-10" />,
  command: <path d="M9 6a3 3 0 1 0-3 3h12a3 3 0 1 0-3-3v12a3 3 0 1 0 3-3H6a3 3 0 1 0 3 3z" />,
  menu: <path d="M4 7h16M4 12h16M4 17h16" />,
  close: <path d="M6 6l12 12M18 6 6 18" />,
  external: (
    <>
      <path d="M14 4h6v6" />
      <path d="M20 4 10 14" />
      <path d="M19 14v5a1 1 0 0 1-1 1H5a1 1 0 0 1-1-1V6a1 1 0 0 1 1-1h5" />
    </>
  ),
  collapse: <path d="m15 6-6 6 6 6" />,
  expand: <path d="m9 6 6 6-6 6" />,
};
