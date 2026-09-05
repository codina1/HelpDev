import { describe, expect, it } from "vitest";
import { mapPublishedContentToMarketplace } from "@/lib/public/map-published-articles";
import type { ContentSummaryDto } from "@/lib/api/content";

describe("mapPublishedContentToMarketplace", () => {
  it("keeps only article/news types and preserves slug/title", () => {
    const items: ContentSummaryDto[] = [
      {
        id: "1",
        title: "تست React",
        slug: "test-react",
        type: "Article",
        status: "Published",
        views: 12,
        saves: 0,
        createdAt: "2026-08-24T00:00:00Z",
        coverImage: "https://api.helpdev.ir/media/cover.png",
      },
      {
        id: "2",
        title: "ابزار",
        slug: "tool-x",
        type: "Tool",
        status: "Published",
        views: 1,
        saves: 0,
        createdAt: "2026-08-24T00:00:00Z",
      },
    ];

    const mapped = mapPublishedContentToMarketplace(items);
    expect(mapped).toHaveLength(1);
    expect(mapped[0].slug).toBe("test-react");
    expect(mapped[0].title).toBe("تست React");
    expect(mapped[0].category).toBe("frontend");
    expect(mapped[0].views).toBe(12);
  });
});
