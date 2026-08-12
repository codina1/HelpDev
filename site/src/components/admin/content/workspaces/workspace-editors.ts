import type { ComponentType } from "react";
import type { ContentWorkspaceId } from "@/lib/admin/content/registry";
import { ArticleEditor } from "@/components/admin/content/workspaces/article/article-editor";
import { ArticleList } from "@/components/admin/content/workspaces/article/article-list";
import { NewsEditor } from "@/components/admin/content/workspaces/news/news-editor";
import { NewsList } from "@/components/admin/content/workspaces/news/news-list";
import { ToolEditor, ToolList } from "@/components/admin/content/workspaces/tool/tool-editor";
import { RoadmapEditor, RoadmapList } from "@/components/admin/content/workspaces/roadmap/roadmap-editor";
import { PromptEditor } from "@/components/admin/content/workspaces/prompt/prompt-editor";
import { PromptList } from "@/components/admin/content/workspaces/prompt/prompt-list";
import {
  ComparisonEditor,
  ComparisonList,
} from "@/components/admin/content/workspaces/comparison/comparison-editor";
import {
  TutorialEditor,
  TutorialList,
} from "@/components/admin/content/workspaces/tutorial/tutorial-editor";

/** Sprint 47A — registry UI wiring: id → editor/list components. */
export const WORKSPACE_EDITORS: Record<ContentWorkspaceId, ComponentType> = {
  article: ArticleEditor,
  news: NewsEditor,
  tool: ToolEditor,
  roadmap: RoadmapEditor,
  prompt: PromptEditor,
  comparison: ComparisonEditor,
  tutorial: TutorialEditor,
};

export const WORKSPACE_LISTS: Record<ContentWorkspaceId, ComponentType> = {
  article: ArticleList,
  news: NewsList,
  tool: ToolList,
  roadmap: RoadmapList,
  prompt: PromptList,
  comparison: ComparisonList,
  tutorial: TutorialList,
};

export function resolveWorkspaceEditor(id: ContentWorkspaceId): ComponentType {
  return WORKSPACE_EDITORS[id];
}

export function resolveWorkspaceList(id: ContentWorkspaceId): ComponentType {
  return WORKSPACE_LISTS[id];
}
