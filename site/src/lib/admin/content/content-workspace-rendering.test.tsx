/**
 * @vitest-environment jsdom
 */
import { describe, expect, it, vi } from "vitest";
import { createRoot } from "react-dom/client";
import { act, type ReactNode } from "react";
import { readFileSync, existsSync } from "node:fs";
import { join } from "node:path";

vi.mock("next/link", () => ({
  default: ({
    href,
    children,
    ...rest
  }: {
    href: string;
    children: ReactNode;
    className?: string;
  }) => (
    <a href={href} {...rest}>
      {children}
    </a>
  ),
}));

vi.mock("next/navigation", () => ({
  useRouter: () => ({ push: vi.fn(), replace: vi.fn(), prefetch: vi.fn() }),
  useSearchParams: () => new URLSearchParams(),
  usePathname: () => "/admin/content/tools",
}));

vi.mock("@/components/auth", () => ({
  useAuth: () => ({ token: null, user: null }),
}));

import { FutureSaveBar } from "@/components/admin/content/workspaces/foundation-workspace-shell";
import { ToolEditor } from "@/components/admin/content/workspaces/tool/tool-editor";
import { ToolFeaturesEditor } from "@/components/admin/content/workspaces/tool/tool-features-editor";
import { RoadmapEditor } from "@/components/admin/content/workspaces/roadmap/roadmap-editor";
import {
  ComparisonList,
  ComparisonEditor,
} from "@/components/admin/content/workspaces/comparison/comparison-editor";
import {
  TutorialList,
  TutorialEditor,
} from "@/components/admin/content/workspaces/tutorial/tutorial-editor";
import { ArticleEditor } from "@/components/admin/content/workspaces/article/article-editor";
import { PromptList } from "@/components/admin/content/workspaces/prompt/prompt-list";
import { ContentPlatformHub } from "@/components/admin/content/content-platform-hub";
import { WORKSPACE_EDITORS, WORKSPACE_LISTS } from "@/components/admin/content/workspaces/workspace-editors";
import { CONTENT_WORKSPACE_IDS } from "@/lib/admin/content/registry";

function render(ui: ReactNode) {
  const host = document.createElement("div");
  document.body.appendChild(host);
  const root = createRoot(host);
  act(() => {
    root.render(ui);
  });
  return {
    host,
    unmount: () => {
      act(() => root.unmount());
      host.remove();
    },
  };
}

describe("Sprint 47A — workspace rendering", () => {
  it("wires a list + editor component for every registry id", () => {
    for (const id of CONTENT_WORKSPACE_IDS) {
      expect(WORKSPACE_LISTS[id]).toBeTypeOf("function");
      expect(WORKSPACE_EDITORS[id]).toBeTypeOf("function");
    }
  });

  it("renders content platform hub with all workspace titles", () => {
    const { host, unmount } = render(<ContentPlatformHub />);
    expect(host.textContent).toContain("پلتفرم محتوا");
    expect(host.textContent).toContain("مقالات");
    expect(host.textContent).toContain("مقایسه‌ها");
    expect(host.textContent).toContain("آموزش‌ها");
    unmount();
  });

  it("renders the remaining foundation list", () => {
    const { host, unmount } = render(<ComparisonList />);
    expect(host.textContent?.length).toBeGreaterThan(20);
    expect(host.querySelector("a")).toBeTruthy();
    unmount();
  });

  it("renders tool editor catalog fields (content-api persistence)", () => {
    const { host, unmount } = render(<ToolEditor />);
    expect(host.textContent).toContain("نام");
    expect(host.textContent).toContain("وب‌سایت");
    expect(host.textContent).toContain("دسته");
    expect(host.textContent).toContain("قیمت‌گذاری");
    expect(host.textContent).toContain("پلتفرم");
    unmount();
  });

  it("renders tool features editor", () => {
    const { host, unmount } = render(
      <ToolFeaturesEditor
        features={[{ id: "1", title: "Composer", description: null, order: 0 }]}
        onAdd={async () => undefined}
        onRemove={async () => undefined}
      />,
    );
    expect(host.textContent).toContain("Composer");
    expect(host.textContent).toContain("افزودن ویژگی");
    unmount();
  });

  it("renders roadmap editor catalog fields (content-api persistence)", () => {
    const { host, unmount } = render(<RoadmapEditor />);
    expect(host.textContent).toContain("سطح");
    expect(host.textContent).toContain("مدت");
    expect(host.textContent).toContain("هدف");
    unmount();
  });

  it("renders comparison create foundation and defines persisted tutorial UI", () => {
    const cmp = render(<ComparisonEditor />);
    expect(cmp.host.textContent).toContain("مقایسه");
    cmp.unmount();
    expect(TutorialEditor).toBeTypeOf("function");
    expect(TutorialList).toBeTypeOf("function");
  });

  it("shows future-save message without inventing persistence", () => {
    const { host, unmount } = render(<FutureSaveBar label="ذخیره" />);
    const button = host.querySelector("button");
    expect(button).toBeTruthy();
    act(() => {
      button!.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    });
    expect(host.textContent).toContain("در نسخه آینده فعال می‌شود");
    unmount();
  });

  it("keeps article editor on the type-locked Content Studio", () => {
    const source = readFileSync(
      join(process.cwd(), "src/components/admin/content/workspaces/article/article-editor.tsx"),
      "utf8",
    );
    expect(source).toContain('createType="Article"');
    expect(source).toContain("ContentStudio");
  });

  it("keeps prompt workspace as Prompt Lab bridge", () => {
    const { host, unmount } = render(<PromptList />);
    expect(host.textContent).toContain("Prompt Lab");
    unmount();
  });

  it("keeps foundation sources free of content create API calls", () => {
    const roots = [
      "src/components/admin/content/workspaces/comparison/comparison-editor.tsx",
      "src/components/admin/content/workspaces/foundation-workspace-shell.tsx",
    ];
    for (const rel of roots) {
      const abs = join(process.cwd(), rel);
      expect(existsSync(abs), rel).toBe(true);
      const source = readFileSync(abs, "utf8");
      expect(source).not.toMatch(/createAdminContent|useCreateAdminContent|postAdminContent/i);
    }
  });

  it("article editor component is defined", () => {
    expect(ArticleEditor).toBeTypeOf("function");
  });
});
