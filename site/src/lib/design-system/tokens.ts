/**
 * HelpDev Premium Design System tokens (Sprint 50D-1).
 * CSS variables in globals.css are the runtime source of truth;
 * this module documents and re-exports values for TS consumers / showcase.
 */

export const dsColors = {
  background: "#060816",
  backgroundElevated: "#0B1020",
  surface: "#111827",
  surfaceElevated: "#151C2C",
  foreground: "#F1F5F9",
  muted: "#94A3B8",
  primary: "#8B5CF6",
  primaryStrong: "#7C3AED",
  secondary: "#06B6D4",
  border: "rgba(255, 255, 255, 0.08)",
  borderStrong: "rgba(255, 255, 255, 0.14)",
  danger: "#F43F5E",
  success: "#34D399",
  warning: "#FBBF24",
} as const;

export const dsTypography = {
  fontFamily: "var(--font-vazirmatn), Tahoma, system-ui, sans-serif",
  display: { size: "2.75rem", weight: 800, lineHeight: 1.25 },
  h1: { size: "2rem", weight: 800, lineHeight: 1.3 },
  h2: { size: "1.5rem", weight: 800, lineHeight: 1.35 },
  h3: { size: "1.125rem", weight: 700, lineHeight: 1.4 },
  body: { size: "0.9375rem", weight: 400, lineHeight: 1.75 },
  caption: { size: "0.75rem", weight: 600, lineHeight: 1.4 },
} as const;

export const dsSpacing = {
  xs: "0.25rem",
  sm: "0.5rem",
  md: "0.75rem",
  lg: "1rem",
  xl: "1.5rem",
  "2xl": "2rem",
  "3xl": "3rem",
} as const;

export const dsRadius = {
  sm: "0.5rem",
  md: "0.75rem",
  lg: "1rem",
  xl: "1.25rem",
  full: "9999px",
} as const;

export const dsShadows = {
  sm: "0 8px 24px rgba(2, 6, 23, 0.35)",
  md: "0 18px 50px rgba(2, 6, 23, 0.55)",
  glow: "0 0 28px rgba(139, 92, 246, 0.35)",
  glowCyan: "0 0 24px rgba(6, 182, 212, 0.28)",
  hover: "0 22px 60px rgba(76, 29, 149, 0.28)",
} as const;

export const dsAnimations = {
  hoverLift: "ds-hover-lift",
  glow: "ds-glow",
  fade: "ds-fade",
  slide: "ds-slide",
} as const;

export const designSystem = {
  colors: dsColors,
  typography: dsTypography,
  spacing: dsSpacing,
  radius: dsRadius,
  shadows: dsShadows,
  animations: dsAnimations,
} as const;
