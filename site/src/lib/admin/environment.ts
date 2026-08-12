import type { AdminNavTone } from "@/lib/admin/navigation";

/**
 * Deployment environment shown in the Admin header. Sourced only from safe,
 * public build-time configuration (`NEXT_PUBLIC_HELPDEV_ENV`) with a fallback to
 * `NODE_ENV`. It never reads secrets and never trusts query strings.
 */
export type AdminEnvironment = "development" | "staging" | "production";

export type AdminEnvironmentMeta = {
  id: AdminEnvironment;
  label: string;
  tone: AdminNavTone;
  description: string;
};

const ENVIRONMENT_META: Record<AdminEnvironment, AdminEnvironmentMeta> = {
  development: {
    id: "development",
    label: "Development",
    tone: "info",
    description: "محیط توسعه — داده‌ها آزمایشی هستند.",
  },
  staging: {
    id: "staging",
    label: "Staging",
    tone: "warning",
    description: "محیط پیش‌انتشار — مشابه تولید اما ایزوله.",
  },
  production: {
    id: "production",
    label: "Production",
    tone: "danger",
    description: "محیط تولید — تغییرات روی کاربران واقعی اثر دارد.",
  },
};

export function resolveAdminEnvironment(
  raw: string | undefined = process.env.NEXT_PUBLIC_HELPDEV_ENV ??
    process.env.NODE_ENV,
): AdminEnvironment {
  const value = (raw ?? "").toLowerCase();
  if (value.startsWith("prod")) return "production";
  if (value.startsWith("stag")) return "staging";
  return "development";
}

export function getAdminEnvironmentMeta(
  environment: AdminEnvironment = resolveAdminEnvironment(),
): AdminEnvironmentMeta {
  return ENVIRONMENT_META[environment];
}
