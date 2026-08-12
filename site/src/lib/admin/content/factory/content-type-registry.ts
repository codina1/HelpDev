/**
 * @deprecated Prefer `@/lib/admin/content/registry` (Sprint 47A).
 * Compatibility shim mapping the platform registry to the Sprint 46.6 shape.
 */
import {
  ContentWorkspaceRegistry,
  getContentWorkspace,
  type ContentWorkspaceId,
  type ContentWorkspaceRegistryEntry,
} from "@/lib/admin/content/registry";
import type { ContentTypeValue } from "@/lib/admin/content/content-types";
import type { AdminIconName } from "@/lib/admin/navigation";
import type { ComponentType } from "react";

export type ContentWorkspaceKey = ContentWorkspaceId;

export type ContentWorkspaceDefinition = {
  key: ContentWorkspaceKey;
  contentType: ContentTypeValue | "none";
  title: string;
  description: string;
  createTitle: string;
  icon: AdminIconName;
  listHref: string;
  createHref: string;
  delegatesToPromptLab?: boolean;
  futureCapabilities?: readonly string[];
  persistence: ContentWorkspaceRegistryEntry["persistence"];
};

function toDefinition(entry: ContentWorkspaceRegistryEntry): ContentWorkspaceDefinition {
  return {
    key: entry.id,
    contentType: entry.contentType ?? "none",
    title: entry.title,
    description: entry.description,
    createTitle: entry.createLabel,
    icon: entry.icon,
    listHref: entry.route,
    createHref: entry.createRoute,
    delegatesToPromptLab: entry.persistence === "prompt-lab",
    persistence: entry.persistence,
  };
}

export const CONTENT_WORKSPACE_KEYS = [
  "article",
  "news",
  "tool",
  "roadmap",
  "prompt",
  "comparison",
  "tutorial",
] as const satisfies readonly ContentWorkspaceKey[];

export const CONTENT_TYPE_REGISTRY: Record<ContentWorkspaceKey, ContentWorkspaceDefinition> =
  Object.fromEntries(
    CONTENT_WORKSPACE_KEYS.map((id) => [id, toDefinition(ContentWorkspaceRegistry[id])]),
  ) as Record<ContentWorkspaceKey, ContentWorkspaceDefinition>;

export function getWorkspaceByKey(key: ContentWorkspaceKey): ContentWorkspaceDefinition {
  return toDefinition(getContentWorkspace(key));
}

export function getWorkspaceByContentType(
  type: ContentTypeValue,
): ContentWorkspaceDefinition | undefined {
  const entry = Object.values(ContentWorkspaceRegistry).find((w) => w.contentType === type);
  return entry ? toDefinition(entry) : undefined;
}

export function resolveEditorKey(key: ContentWorkspaceKey): ContentWorkspaceKey {
  return key;
}

export type WorkspaceEditorMap = Partial<Record<ContentWorkspaceKey, ComponentType>>;
