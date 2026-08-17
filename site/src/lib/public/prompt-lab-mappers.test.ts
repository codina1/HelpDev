import { describe, expect, it } from "vitest";
import { HOME_COVERS } from "@/lib/public/home-covers";
import type { PublicPromptDetailsDto, PublicPromptListItemDto } from "@/lib/api/promptlab";
import { PROMPT_LAB_LIBRARY_AUTHOR, toPromptLabCardItem, toPromptLabDetail } from "@/lib/public/prompt-lab-mappers";

function listItem(overrides: Partial<PublicPromptListItemDto> = {}): PublicPromptListItemDto {
  return {
    id: "11111111-1111-1111-1111-111111111111",
    title: "بازبینی مرز ماژول",
    slug: "system-boundary-review",
    description: "پرامپت بررسی قرارداد دامنه.",
    coverImage: "/home/cover-architecture.svg",
    mediaType: "Text",
    category: { id: "c1", name: "Coding", slug: "coding" },
    aiModel: { id: "m1", name: "Claude", slug: "claude", provider: "Anthropic" },
    views: 1240,
    copyCount: 186,
    publishedAt: "2026-08-16T08:00:00.000Z",
    ...overrides,
  };
}

describe("prompt lab API mappers", () => {
  it("maps list items to card fields with Persian category labels", () => {
    const item = toPromptLabCardItem(listItem());
    expect(item.title).toBe("بازبینی مرز ماژول");
    expect(item.category).toBe("کدنویسی");
    expect(item.aiModel).toBe("Claude");
    expect(item.viewCount).toBe(1240);
    expect(item.copyCount).toBe(186);
    expect(item.coverImage).toBe("/home/cover-architecture.svg");
  });

  it("fills empty cover and description from local fallbacks", () => {
    const item = toPromptLabCardItem(listItem({ coverImage: null, description: null }));
    expect(item.coverImage).toBe(HOME_COVERS.architecture);
    expect(item.description).toBe("بازبینی مرز ماژول");
  });

  it("maps detail content, tags, and library author", () => {
    const detail = toPromptLabDetail({
      ...listItem(),
      content: "You are a staff engineer.",
    } satisfies PublicPromptDetailsDto);
    expect(detail.content).toContain("staff engineer");
    expect(detail.author).toEqual(PROMPT_LAB_LIBRARY_AUTHOR);
    expect(detail.tags).toContain("کدنویسی");
    expect(detail.tags).toContain("Claude");
    expect(detail.mediaType).toBe("Text");
  });
});
