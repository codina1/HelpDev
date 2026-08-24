import { describe, expect, it } from "vitest";
import {
  buildSeoPayload,
  filterContent,
  formatDateFa,
  formatDateTimeFa,
  groupSeoAnalysisFindings,
  isAbsoluteUrl,
  isValidSlug,
  resolveContentCoverUrl,
  labelForContentStatus,
  labelForContentType,
  labelForNewsPriority,
  labelForSeoFindingCategory,
  labelForSeoFindingSeverity,
  mapAdminContentDetail,
  mapAdminContentListItem,
  mapAdminContentPagedResult,
  mapContentDetail,
  mapContentList,
  mapContentStats,
  mapSeoAnalysisFinding,
  mapSeoAnalysisReport,
  mapSeoForm,
  normalizeContentStatus,
  shortAuthorId,
  slugify,
  validateContentForm,
  validateSeoForm,
} from "./content-mappers";
import {
  EMPTY_SEO_FORM,
  SEO_LIMITS,
  type AdminContentDetailRawDto,
  type ContentDetailRawDto,
  type ContentFormValues,
  type ContentListItem,
  type ContentListRawDto,
  type SeoAuditFindingRawDto,
  type SeoAuditReportRawDto,
  type SeoFormValues,
} from "./content-types";
import type { AdminDashboardDto } from "@/lib/admin/dashboard/dashboard-types";

const validForm: ContentFormValues = {
  title: "اولین مقاله",
  slug: "first-article",
  type: "Article",
  body: "متن نمونه",
  status: "Draft",
  excerpt: "",
  coverImage: "",
};

describe("slugify / isValidSlug", () => {
  it("produces backend-valid slugs from latin text", () => {
    expect(slugify("Hello World!")).toBe("hello-world");
    expect(slugify("  Multiple   Spaces  ")).toBe("multiple-spaces");
    expect(slugify("Already-Valid-123")).toBe("already-valid-123");
  });

  it("returns an empty string for non-latin input", () => {
    expect(slugify("سلام دنیا")).toBe("");
  });

  it("validates against the backend slug pattern", () => {
    expect(isValidSlug("first-article")).toBe(true);
    expect(isValidSlug("a")).toBe(false); // too short
    expect(isValidSlug("Has-Upper")).toBe(false);
    expect(isValidSlug("bad slug")).toBe(false);
    expect(isValidSlug("-leading")).toBe(false);
  });
});

describe("validateContentForm", () => {
  it("passes a valid form", () => {
    expect(validateContentForm(validForm)).toEqual({});
  });

  it("flags a missing title", () => {
    expect(validateContentForm({ ...validForm, title: "  " }).title).toBeTruthy();
  });

  it("flags an invalid slug", () => {
    expect(validateContentForm({ ...validForm, slug: "Bad Slug" }).slug).toBeTruthy();
  });

  it("flags an empty body", () => {
    expect(validateContentForm({ ...validForm, body: "" }).body).toBeTruthy();
  });

  it("flags an unknown content type", () => {
    expect(
      validateContentForm({
        ...validForm,
        type: "Unknown" as ContentFormValues["type"],
      }).type,
    ).toBeTruthy();
  });

  it("flags an over-long excerpt", () => {
    const excerpt = "a".repeat(501);
    expect(validateContentForm({ ...validForm, excerpt }).excerpt).toBeTruthy();
  });

  it("flags a non-absolute cover image url", () => {
    expect(validateContentForm({ ...validForm, coverImage: "not-a-url" }).coverImage).toBeTruthy();
    expect(validateContentForm({ ...validForm, coverImage: "https://x.dev/a.png" }).coverImage).toBeUndefined();
  });
});

describe("isAbsoluteUrl", () => {
  it("accepts only http(s) absolute urls", () => {
    expect(isAbsoluteUrl("https://example.com/a")).toBe(true);
    expect(isAbsoluteUrl("http://example.com")).toBe(true);
    expect(isAbsoluteUrl("ftp://example.com")).toBe(false);
    expect(isAbsoluteUrl("/relative")).toBe(false);
    expect(isAbsoluteUrl("example.com")).toBe(false);
    expect(isAbsoluteUrl("javascript:alert(1)")).toBe(false);
  });
});

describe("resolveContentCoverUrl", () => {
  it("prefers the content cover over the OG image", () => {
    expect(
      resolveContentCoverUrl(
        "https://cdn.example/cover.png",
        "https://cdn.example/og.png",
      ),
    ).toBe("https://cdn.example/cover.png");
  });

  it("falls back to the OG image when the cover is missing", () => {
    expect(resolveContentCoverUrl("", "https://cdn.example/og.png")).toBe(
      "https://cdn.example/og.png",
    );
    expect(resolveContentCoverUrl(null, "https://cdn.example/og.png")).toBe(
      "https://cdn.example/og.png",
    );
  });

  it("returns empty when neither cover nor OG image is set", () => {
    expect(resolveContentCoverUrl("", "")).toBe("");
  });
});

describe("validateSeoForm", () => {
  const base: SeoFormValues = { ...EMPTY_SEO_FORM };

  it("passes empty metadata (all fields optional)", () => {
    expect(validateSeoForm(base)).toEqual({});
  });

  it("passes valid metadata", () => {
    expect(
      validateSeoForm({
        seoTitle: "A good title",
        seoDescription: "A concise description.",
        canonicalUrl: "https://helpdev.example/a",
        ogImage: "https://cdn.helpdev.example/og.png",
        focusKeyword: "helpdev",
      }),
    ).toEqual({});
  });

  it("flags an over-long SEO title", () => {
    const seoTitle = "t".repeat(SEO_LIMITS.seoTitle + 1);
    expect(validateSeoForm({ ...base, seoTitle }).seoTitle).toBeTruthy();
  });

  it("flags an over-long SEO description", () => {
    const seoDescription = "d".repeat(SEO_LIMITS.seoDescription + 1);
    expect(validateSeoForm({ ...base, seoDescription }).seoDescription).toBeTruthy();
  });

  it("flags an invalid canonical url", () => {
    expect(validateSeoForm({ ...base, canonicalUrl: "not-a-url" }).canonicalUrl).toBeTruthy();
    expect(validateSeoForm({ ...base, canonicalUrl: "ftp://x.dev" }).canonicalUrl).toBeTruthy();
  });

  it("flags an invalid OG image url", () => {
    expect(validateSeoForm({ ...base, ogImage: "nope" }).ogImage).toBeTruthy();
  });
});

describe("buildSeoPayload", () => {
  it("trims values and maps blanks to null", () => {
    expect(
      buildSeoPayload({
        seoTitle: "  Title  ",
        seoDescription: "   ",
        canonicalUrl: "https://x.dev/a",
        ogImage: "",
        focusKeyword: "  kw ",
      }),
    ).toEqual({
      seoTitle: "Title",
      seoDescription: null,
      canonicalUrl: "https://x.dev/a",
      ogImage: null,
      focusKeyword: "kw",
    });
  });
});

describe("mapSeoForm", () => {
  it("maps an admin detail DTO into SEO form values (nulls → empty strings)", () => {
    const dto = {
      seo: {
        seoTitle: "T",
        seoDescription: null,
        canonicalUrl: "https://x.dev",
        ogImage: null,
        focusKeyword: "k",
      },
    } as AdminContentDetailRawDto;
    expect(mapSeoForm(dto)).toEqual({
      seoTitle: "T",
      seoDescription: "",
      canonicalUrl: "https://x.dev",
      ogImage: "",
      focusKeyword: "k",
    });
  });
});

describe("mapAdminContentPagedResult", () => {
  const raw = {
    items: [
      {
        id: "a1",
        title: "پیش‌نویس",
        slug: "draft-one",
        contentType: "Article",
        contentStatus: "Draft",
        authorId: "11111111-1111-1111-1111-111111111111",
        createdAtUtc: "2026-07-01T00:00:00Z",
        updatedAtUtc: "2026-07-02T00:00:00Z",
        publishedAtUtc: null,
      },
      {
        id: "a2",
        title: "منتشرشده",
        slug: "pub-one",
        contentType: "News",
        contentStatus: "Published",
        authorId: "22222222-2222-2222-2222-222222222222",
        createdAtUtc: "2026-07-01T00:00:00Z",
        updatedAtUtc: "2026-07-03T00:00:00Z",
        publishedAtUtc: "2026-07-03T00:00:00Z",
      },
    ],
    page: 1,
    pageSize: 20,
    totalCount: 2,
    totalPages: 1,
  };

  it("maps list items including null publishedAt and enum labels", () => {
    const page = mapAdminContentPagedResult(raw);
    expect(page.totalCount).toBe(2);
    expect(page.items[0]?.status).toBe("Draft");
    expect(page.items[0]?.statusLabel).toBe("پیش‌نویس");
    expect(page.items[0]?.publishedAtUtc).toBeNull();
    expect(page.items[0]?.typeLabel).toBe("مقاله");
    expect(page.items[1]?.status).toBe("Published");
    expect(page.items[1]?.publishedAtUtc).toBe("2026-07-03T00:00:00Z");
  });

  it("computes totalPages when missing", () => {
    const page = mapAdminContentPagedResult({
      ...raw,
      totalCount: 45,
      pageSize: 20,
      totalPages: undefined as unknown as number,
    });
    expect(page.totalPages).toBe(3);
  });

  it("mapAdminContentListItem normalizes status case-insensitively", () => {
    const item = mapAdminContentListItem({
      ...raw.items[0]!,
      contentStatus: "published",
    });
    expect(item.status).toBe("Published");
  });
});

describe("mapAdminContentDetail", () => {
  const dto: AdminContentDetailRawDto = {
    id: "c1",
    title: "مقاله ادمین",
    slug: "admin-article",
    body: "# سرفصل\nمتن",
    excerpt: "خلاصه",
    coverImage: null,
    contentType: "Article",
    contentStatus: "Published",
    authorId: "22222222-2222-2222-2222-222222222222",
    views: 5,
    saves: 1,
    createdAtUtc: "2026-07-01T00:00:00Z",
    updatedAtUtc: "2026-07-02T00:00:00Z",
    publishedAtUtc: "2026-07-02T00:00:00Z",
    seo: {
      seoTitle: "عنوان سئو",
      seoDescription: null,
      canonicalUrl: "https://helpdev.example/admin-article",
      ogImage: null,
      focusKeyword: "کلیدواژه",
    },
  };

  it("maps the full admin read model including SEO and timestamps", () => {
    const detail = mapAdminContentDetail(dto);
    expect(detail.id).toBe("c1");
    expect(detail.title).toBe("مقاله ادمین");
    expect(detail.slug).toBe("admin-article");
    expect(detail.body).toBe("# سرفصل\nمتن");
    expect(detail.excerpt).toBe("خلاصه");
    expect(detail.type).toBe("Article");
    expect(detail.typeLabel).toBe("مقاله");
    expect(detail.status).toBe("Published");
    expect(detail.statusLabel).toBe("منتشرشده");
    expect(detail.createdAtUtc).toBe("2026-07-01T00:00:00Z");
    expect(detail.updatedAtUtc).toBe("2026-07-02T00:00:00Z");
    expect(detail.publishedAtUtc).toBe("2026-07-02T00:00:00Z");
  });

  it("coerces null cover image and SEO fields to empty strings for the form", () => {
    const detail = mapAdminContentDetail(dto);
    expect(detail.coverImage).toBe("");
    expect(detail.seo.seoTitle).toBe("عنوان سئو");
    expect(detail.seo.seoDescription).toBe("");
    expect(detail.seo.ogImage).toBe("");
    expect(detail.seo.canonicalUrl).toBe("https://helpdev.example/admin-article");
    expect(detail.seo.focusKeyword).toBe("کلیدواژه");
  });
});

describe("labels", () => {
  it("maps known content types and statuses to Persian", () => {
    expect(labelForContentType("Article")).toBe("مقاله");
    expect(labelForContentType("RoadmapStep")).toBe("گام نقشه راه (قدیمی)");
    expect(labelForContentType("Roadmap")).toBe("نقشه راه");
    expect(labelForContentType("Custom")).toBe("Custom");
    expect(labelForContentStatus("Published")).toBe("منتشرشده");
    expect(labelForContentStatus("Draft")).toBe("پیش‌نویس");
    expect(labelForContentStatus("ReviewPending")).toBe("در انتظار بررسی");
    expect(labelForContentStatus("Approved")).toBe("تأییدشده");
    expect(labelForContentStatus("Archived")).toBe("بایگانی‌شده");
    expect(labelForNewsPriority("Normal")).toBe("عادی");
    expect(labelForNewsPriority("Featured")).toBe("ویژه");
    expect(labelForNewsPriority("Breaking")).toBe("فوری");
  });

  it("normalizes status values case-insensitively", () => {
    expect(normalizeContentStatus("published")).toBe("Published");
    expect(normalizeContentStatus("DRAFT")).toBe("Draft");
    expect(normalizeContentStatus("reviewpending")).toBe("ReviewPending");
    expect(normalizeContentStatus("weird")).toBe("Draft");
  });
});

describe("mapContentList / mapContentDetail", () => {
  const listDto: ContentListRawDto[] = [
    {
      id: "1",
      title: "مقاله",
      slug: "an-article",
      type: "Article",
      authorId: "11111111-1111-1111-1111-111111111111",
      views: 10,
      saves: 2,
      createdAt: "2026-07-01T00:00:00Z",
    },
  ];

  it("marks list items as published (list endpoint is published-only)", () => {
    const mapped = mapContentList(listDto);
    expect(mapped[0].status).toBe("Published");
    expect(mapped[0].statusLabel).toBe("منتشرشده");
    expect(mapped[0].typeLabel).toBe("مقاله");
  });

  it("maps detail and normalizes status", () => {
    const detailDto: ContentDetailRawDto = {
      id: "1",
      title: "مقاله",
      slug: "an-article",
      body: "بدنه",
      type: "News",
      authorId: "abc",
      status: "Draft",
      views: 0,
      saves: 0,
      createdAt: "2026-07-01T00:00:00Z",
    };
    const mapped = mapContentDetail(detailDto);
    expect(mapped.status).toBe("Draft");
    expect(mapped.typeLabel).toBe("خبر");
    expect(mapped.body).toBe("بدنه");
  });

  it("maps public coverImage onto list and detail view models", () => {
    const mappedList = mapContentList([
      {
        ...listDto[0],
        coverImage: "/media/2026/08/cover.png",
      },
    ]);
    expect(mappedList[0].coverImage).toBe("/media/2026/08/cover.png");

    const mappedDetail = mapContentDetail({
      id: "1",
      title: "مقاله",
      slug: "an-article",
      body: "بدنه",
      type: "Article",
      authorId: "abc",
      status: "Published",
      views: 0,
      saves: 0,
      createdAt: "2026-07-01T00:00:00Z",
      coverImage: "/media/2026/08/cover.png",
    });
    expect(mappedDetail.coverImage).toBe("/media/2026/08/cover.png");
  });
});

describe("mapContentStats", () => {
  it("derives content stats from the dashboard DTO", () => {
    const dto = {
      content: {
        totalContent: 100,
        publishedContent: 80,
        draftContent: 20,
        publicationsToday: 3,
      },
    } as AdminDashboardDto;
    expect(mapContentStats(dto)).toEqual({
      total: 100,
      published: 80,
      draft: 20,
      publicationsToday: 3,
    });
  });
});

describe("filterContent", () => {
  const items: ContentListItem[] = [
    {
      id: "1",
      title: "React 19",
      slug: "react-19",
      type: "News",
      typeLabel: "خبر",
      authorId: "a",
      views: 0,
      saves: 0,
      createdAt: "",
      status: "Published",
      statusLabel: "منتشرشده",
    },
    {
      id: "2",
      title: "ابزار Git",
      slug: "git-tool",
      type: "Tool",
      typeLabel: "ابزار",
      authorId: "b",
      views: 0,
      saves: 0,
      createdAt: "",
      status: "Published",
      statusLabel: "منتشرشده",
    },
  ];

  it("filters by search across title and slug", () => {
    expect(filterContent(items, { search: "git", type: "all", status: "all" })).toHaveLength(1);
    expect(filterContent(items, { search: "react", type: "all", status: "all" })).toHaveLength(1);
  });

  it("filters by type", () => {
    expect(filterContent(items, { search: "", type: "Tool", status: "all" })).toHaveLength(1);
  });

  it("returns empty when filtering by an absent status (list is published-only)", () => {
    expect(filterContent(items, { search: "", type: "all", status: "Draft" })).toHaveLength(0);
  });
});

describe("misc formatting", () => {
  it("formats dates and shortens author ids", () => {
    expect(formatDateFa("not-a-date")).toBe("");
    expect(formatDateFa("2026-07-01T00:00:00Z")).toMatch(/[۰-۹]/);
    expect(shortAuthorId("1234567890")).toBe("12345678…");
    expect(shortAuthorId("short")).toBe("short");
  });

  it("formats analysis timestamps with date and time", () => {
    expect(formatDateTimeFa("not-a-date")).toBe("");
    expect(formatDateTimeFa("2026-07-01T10:30:00Z")).toMatch(/[۰-۹]/);
  });
});

describe("SEO analysis mapping (POST /admin/content/{id}/seo-analysis)", () => {
  const findingDto = (
    overrides: Partial<SeoAuditFindingRawDto> = {},
  ): SeoAuditFindingRawDto => ({
    ruleId: "title-length",
    category: "Metadata",
    severity: "Warning",
    message: "عنوان کوتاه است.",
    suggestion: "عنوان را طولانی‌تر کنید.",
    field: "seoTitle",
    ...overrides,
  });

  it("labels platform categories and severities", () => {
    expect(labelForSeoFindingCategory("Metadata")).toBe("متادیتا");
    expect(labelForSeoFindingCategory("ContentStructure")).toBe("ساختار محتوا");
    expect(labelForSeoFindingCategory("Technical")).toBe("فنی");

    expect(labelForSeoFindingSeverity("Info")).toBe("اطلاعاتی");
    expect(labelForSeoFindingSeverity("Warning")).toBe("هشدار");
    expect(labelForSeoFindingSeverity("Error")).toBe("خطا");
  });

  it("maps a raw audit finding into a view model with labels", () => {
    const mapped = mapSeoAnalysisFinding(findingDto());
    expect(mapped.ruleId).toBe("title-length");
    expect(mapped.passed).toBe(false);
    expect(mapped.suggestion).toBe("عنوان را طولانی‌تر کنید.");
    expect(mapped.categoryLabel).toBe("متادیتا");
  });

  it("maps the full audit report", () => {
    const raw: SeoAuditReportRawDto = {
      contentId: "c-1",
      generatedAtUtc: "2026-07-01T10:00:00Z",
      summary: { errorCount: 1, warningCount: 2, infoCount: 3 },
      findings: [findingDto()],
    };
    const mapped = mapSeoAnalysisReport(raw);
    expect(mapped.contentId).toBe("c-1");
    expect(mapped.analyzedAtUtc).toBe(raw.generatedAtUtc);
    expect(mapped.summary).toEqual(raw.summary);
    expect(mapped.findings).toHaveLength(1);
  });

  it("groups findings by platform category in fixed order", () => {
    const findings = [
      findingDto({ ruleId: "r1", category: "Metadata" }),
      findingDto({ ruleId: "r2", category: "Links" }),
      findingDto({ ruleId: "r3", category: "Technical" }),
    ].map(mapSeoAnalysisFinding);

    const sections = groupSeoAnalysisFindings(findings);
    expect(sections.map((s) => s.key)).toEqual(["Metadata", "Links", "Technical"]);
  });
});
