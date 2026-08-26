import { afterEach, beforeEach, describe, expect, it, vi } from "vitest";
import { fetchPromptLabCatalog, excludePromptSlug, toPromptLabCatalogPage } from "@/lib/public/prompt-lab-catalog";

const listPrompts = vi.fn();

vi.mock("@/lib/api/promptlab", () => ({
  listPrompts: (...args: unknown[]) => listPrompts(...args),
}));

describe("prompt lab catalog", () => {
  beforeEach(() => {
    listPrompts.mockReset();
  });

  afterEach(() => {
    vi.clearAllMocks();
  });

  it("requests GET /prompts filters and maps the page", async () => {
    listPrompts.mockResolvedValueOnce({
      page: 2,
      pageSize: 8,
      total: 20,
      items: [
        {
          id: "1",
          title: "RAG",
          slug: "rag-query-rewrite",
          description: "بازنویسی پرسش",
          coverImage: "/home/icon-ai.png",
          mediaType: "Text",
          category: { id: "c", name: "Coding", slug: "coding" },
          aiModel: { id: "m", name: "Gemini", slug: "gemini", provider: "Google" },
          views: 10,
          copyCount: 2,
          publishedAt: null,
        },
      ],
    });

    const page = await fetchPromptLabCatalog({
      search: "RAG",
      category: "coding",
      page: 2,
      popular: true,
    });

    expect(listPrompts).toHaveBeenCalledWith(
      {
        search: "RAG",
        category: "coding",
        popular: true,
        page: 2,
        pageSize: 8,
      },
      undefined,
    );
    expect(page.total).toBe(20);
    expect(page.items[0]?.slug).toBe("rag-query-rewrite");
    expect(page.items[0]?.category).toBe("کدنویسی");
  });

  it("excludes the current slug from related lists", () => {
    const mapped = toPromptLabCatalogPage({
      page: 1,
      pageSize: 4,
      total: 2,
      items: [],
    });
    expect(mapped.items).toEqual([]);
    expect(
      excludePromptSlug(
        [
          {
            id: "1",
            slug: "keep",
            title: "a",
            description: "a",
            coverImage: "/x.svg",
            aiModel: "Claude",
            category: "کدنویسی",
            categorySlug: "coding",
            copyCount: 0,
            viewCount: 0,
            featured: false,
            publishedAt: "",
          },
          {
            id: "2",
            slug: "drop",
            title: "b",
            description: "b",
            coverImage: "/x.svg",
            aiModel: "Claude",
            category: "کدنویسی",
            categorySlug: "coding",
            copyCount: 0,
            viewCount: 0,
            featured: false,
            publishedAt: "",
          },
        ],
        "drop",
      ).map((item) => item.slug),
    ).toEqual(["keep"]);
  });
});
