import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { describe, expect, it } from "vitest";
import { renderToStaticMarkup } from "react-dom/server";
import { PublicPromptLabPackPage } from "./public-prompt-lab-pack-page";
import { getPromptLabPack } from "@/lib/public/prompt-lab-pack-mock";
import { publicPromptLabPackPath } from "@/lib/public/prompt-lab-routes";

describe("public prompt pack page", () => {
  it("is mounted at /prompt-lab/packs/[slug]", () => {
    expect(publicPromptLabPackPath("modular-monolith-studio")).toBe(
      "/prompt-lab/packs/modular-monolith-studio",
    );
    expect(
      existsSync(join(process.cwd(), "src", "app", "prompt-lab", "packs", "[slug]", "page.tsx")),
    ).toBe(true);
  });

  it("renders pack hero and numbered timeline items from mock data", () => {
    const pack = getPromptLabPack("modular-monolith-studio");
    expect(pack).not.toBeNull();
    const html = renderToStaticMarkup(<PublicPromptLabPackPage pack={pack!} />);
    expect(html).toContain(pack!.title);
    expect(html).toContain(pack!.description);
    expect(html).toContain(pack!.category);
    expect(html).toContain(pack!.coverImage);
    expect(html).toContain("پرامپت");
    expect(html).toContain("فهرست پرامپت‌ها");
    expect(html).toContain(pack!.items[0]!.prompt.title);
    expect(html).toContain(pack!.items[0]!.prompt.description);
    expect(html).toContain(pack!.items[0]!.preview.split("\n")[0]);
    expect(html).toContain("کپی");
    expect(html).toContain("/prompt-lab/system-boundary-review");
    expect(html).toContain('dir="rtl"');
    expect(html).not.toContain("پرامپت با موفقیت کپی شد");
  });

  it("keeps the pack route free of API imports", () => {
    const page = readFileSync(
      join(process.cwd(), "src/app/prompt-lab/packs/[slug]/page.tsx"),
      "utf8",
    );
    expect(page).toContain("getPromptLabPack");
    expect(page).not.toContain("@/lib/api");
    expect(page).not.toContain("promptLabApi");
    expect(page).not.toContain("@/components/admin");
  });
});
