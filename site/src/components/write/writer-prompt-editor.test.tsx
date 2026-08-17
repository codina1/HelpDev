import { describe, expect, it } from "vitest";
import { existsSync, readFileSync } from "node:fs";
import { join } from "node:path";
import { WRITER_PROMPT_NEW_PATH } from "@/lib/admin/prompt-lab/writer-prompt-types";

describe("Writer Prompt Editor", () => {
  it("is mounted at /write/prompts/new", () => {
    expect(WRITER_PROMPT_NEW_PATH).toBe("/write/prompts/new");
    expect(existsSync(join(process.cwd(), "src/app/write/prompts/new/page.tsx"))).toBe(true);
    const page = readFileSync(join(process.cwd(), "src/app/write/prompts/new/page.tsx"), "utf8");
    expect(page).toContain("WriterPromptEditor");
  });

  it("exposes draft and review actions without publish", () => {
    const source = readFileSync(
      join(process.cwd(), "src/components/write/writer-prompt-editor.tsx"),
      "utf8",
    );
    expect(source).toContain("ذخیره پیش‌نویس");
    expect(source).toContain("ارسال برای بررسی");
    expect(source).toContain("createWriterPrompt");
    expect(source).toContain("submitWriterPrompt");
    expect(source).not.toContain("publishWriterPrompt");
    expect(source).toContain("عنوان");
    expect(source).toContain("توضیح");
    expect(source).toContain("تصویر کاور");
    expect(source).toContain("متن پرامپت");
    expect(source).toContain("مدل هوش مصنوعی");
    expect(source).toContain("دسته‌بندی");
    expect(source).toContain("نوع رسانه");
    expect(source).toContain("برچسب‌ها");
  });
});
