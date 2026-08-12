"use client";

import type { ReactNode } from "react";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import type { ToolFeatureDto } from "@/lib/api/content";
import type { ToolFormState } from "@/components/admin/content/workspaces/tool/tool-form-types";

type ToolPreviewProps = {
  tool: ToolFormState & { features?: ToolFeatureDto[] };
  title: string;
  body: string;
  features?: ToolFeatureDto[];
};

export function ToolPreview({ tool, title, body, features }: ToolPreviewProps) {
  const featureList = features ?? tool.features ?? [];
  return (
    <AdminSurface className="space-y-4 p-4">
      <h2 className="adm-text text-[14px] font-bold">پیش‌نمایش کاتالوگ</h2>
      <div className="space-y-2 rounded-xl border border-[var(--adm-border)] bg-[var(--adm-surface-2)] p-4">
        <p className="adm-text text-[16px] font-bold">{tool.toolName || title || "نام ابزار"}</p>
        <p className="adm-muted text-[12px]">{tool.toolCategory || "بدون دسته"}</p>
        <div className="flex flex-wrap gap-2 text-[11px]">
          <Badge>{tool.pricingModel}</Badge>
          <Badge>{tool.licenseType}</Badge>
          {tool.platforms.map((p) => (
            <Badge key={p}>{p}</Badge>
          ))}
        </div>
        {tool.officialWebsiteUrl ? (
          <p className="font-mono text-[11px] text-[var(--adm-accent)]" dir="ltr">
            {tool.officialWebsiteUrl}
          </p>
        ) : null}
        {tool.githubUrl ? (
          <p className="font-mono text-[11px]" dir="ltr">
            {tool.githubUrl}
          </p>
        ) : null}
        {featureList.length > 0 ? (
          <ul className="mt-2 list-disc space-y-1 pr-5 text-[12px]">
            {featureList.map((f) => (
              <li key={f.id}>{f.title}</li>
            ))}
          </ul>
        ) : null}
        {tool.alternatives.length > 0 ? (
          <p className="adm-muted text-[11px]">
            جایگزین‌ها: {tool.alternatives.length} مورد
          </p>
        ) : null}
        {body.trim() ? (
          <p className="adm-muted mt-2 line-clamp-4 whitespace-pre-wrap text-[12px]">{body}</p>
        ) : null}
      </div>
    </AdminSurface>
  );
}

function Badge({ children }: { children: ReactNode }) {
  return (
    <span className="rounded-md bg-[var(--adm-surface)] px-2 py-0.5 font-medium text-[var(--adm-text)]">
      {children}
    </span>
  );
}
