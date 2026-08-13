/**
 * Homepage design tokens. Runtime source of truth is `--home-*` in globals.css.
 * This module mirrors those values for TS consumers. Do not restyle --ds-* / --pub-*.
 */

export const homeColors = {
  background: "#060816",
  backgroundElevated: "#080A12",
  atmospherePurple: "rgba(139, 92, 246, 0.14)",
  atmosphereBlue: "rgba(99, 102, 241, 0.12)",
  atmosphereCyan: "rgba(6, 182, 212, 0.08)",
  surface: "rgba(17, 24, 39, 0.62)",
  surfaceSolid: "#111827",
  surfaceElevated: "rgba(21, 28, 44, 0.78)",
  surfaceHover: "rgba(255, 255, 255, 0.04)",
  border: "rgba(255, 255, 255, 0.08)",
  borderStrong: "rgba(255, 255, 255, 0.14)",
  borderAccent: "rgba(139, 92, 246, 0.35)",
  purple: "#8B5CF6",
  purpleStrong: "#7C3AED",
  purpleSoft: "rgba(139, 92, 246, 0.16)",
  blue: "#6366F1",
  blueSoft: "rgba(99, 102, 241, 0.16)",
  cyan: "#06B6D4",
  cyanSoft: "rgba(6, 182, 212, 0.14)",
  text: "#F8FAFC",
  textSecondary: "#CBD5E1",
  textMuted: "#94A3B8",
  textSubtle: "#64748B",
  textOnAccent: "#FFFFFF",
} as const;

export const homeRadius = {
  sm: "0.5rem",
  md: "0.75rem",
  lg: "1rem",
  xl: "1.25rem",
  "2xl": "1.5rem",
} as const;

export const homeShadows = {
  sm: "0 8px 24px rgba(2, 6, 23, 0.35)",
  md: "0 18px 50px rgba(2, 6, 23, 0.55)",
  glowPurple: "0 0 28px rgba(139, 92, 246, 0.35)",
  glowBlue: "0 0 24px rgba(99, 102, 241, 0.28)",
  glowCyan: "0 0 24px rgba(6, 182, 212, 0.28)",
} as const;

export const homeSpacing = {
  1: "0.25rem",
  2: "0.5rem",
  3: "0.75rem",
  4: "1rem",
  5: "1.25rem",
  6: "1.5rem",
  8: "2rem",
  10: "2.5rem",
  12: "3rem",
  16: "4rem",
  sectionGap: "2.5rem",
  gutter: "1.25rem",
} as const;

export const homeContainer = {
  default: "100%",
  wide: "100%",
  narrow: "48rem",
  headerHeight: "72px",
} as const;

export const homeTypography = {
  fontFamily: "var(--font-vazirmatn), Tahoma, system-ui, sans-serif",
  display: { size: "2.75rem", sizeSm: "2rem", weight: 800, lineHeight: 1.25 },
  title: { size: "1.5rem", weight: 700, lineHeight: 1.35 },
  subtitle: { size: "1.125rem", weight: 700, lineHeight: 1.4 },
  body: { size: "0.9375rem", weight: 400, lineHeight: 1.75 },
  caption: { size: "0.75rem", weight: 600, lineHeight: 1.4 },
} as const;

export const homeTokens = {
  colors: homeColors,
  radius: homeRadius,
  shadows: homeShadows,
  spacing: homeSpacing,
  container: homeContainer,
  typography: homeTypography,
} as const;
