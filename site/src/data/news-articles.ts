import type { NewsArticle, NewsTag } from "@/types";

export const NEWS_TAGS: NewsTag[] = ["React", ".NET", "AI", "DevOps"];

/** Display-only cloud tags used in the sidebar (filter by keyword when possible). */
export const NEWS_CLOUD_TAGS = [
  "همه",
  "AI",
  "Cursor",
  "Claude",
  "NextJS",
  "React",
  ".NET",
  "DevOps",
  "Docker",
  "MCP",
] as const;

export type NewsCloudTag = (typeof NEWS_CLOUD_TAGS)[number];

/** Category pills under the hero — order matches RTL reference (همه first). */
export type NewsCategoryId =
  | "همه"
  | "AI"
  | "Programming"
  | ".NET"
  | "Frontend"
  | "Backend"
  | "DevOps"
  | "Tools"
  | "Security";

export const NEWS_CATEGORY_FILTERS: readonly {
  id: NewsCategoryId;
  label: string;
  icon: string;
}[] = [
  { id: "همه", label: "همه", icon: "all" },
  { id: "AI", label: "AI", icon: "ai" },
  { id: "Programming", label: "Programming", icon: "code" },
  { id: ".NET", label: ".NET", icon: "dotnet" },
  { id: "Frontend", label: "Frontend", icon: "frontend" },
  { id: "Backend", label: "Backend", icon: "backend" },
  { id: "DevOps", label: "DevOps", icon: "devops" },
  { id: "Tools", label: "Tools", icon: "tools" },
  { id: "Security", label: "Security", icon: "security" },
];

export const NEWS_ARTICLES: NewsArticle[] = [
  {
    id: "1",
    title: "React 19 Compiler moves closer to stable",
    tag: "React",
    summary:
      "Automatic memoization lands in more production apps, with clearer guidance on when to keep manual optimizations.",
    time: "1h ago",
    image: "/home/icon-frontend.png",
    readTime: "۵ دقیقه مطالعه",
    views: "۱۲.۴K بازدید",
  },
  {
    id: "2",
    title: ".NET 10 preview focuses on cloud-native APIs",
    tag: ".NET",
    summary:
      "Faster minimal APIs, improved OpenAPI generation, and leaner container images for ASP.NET Core services.",
    time: "2h ago",
    image: "/home/icon-dotnet.png",
    readTime: "۶ دقیقه مطالعه",
    views: "۹.۸K بازدید",
  },
  {
    id: "3",
    title: "AI coding agents get better at multi-file refactors",
    tag: "AI",
    summary:
      "New evaluation suites show stronger results on repository-wide changes, not just single-function edits.",
    time: "3h ago",
    image: "/home/icon-ai.png",
    readTime: "۷ دقیقه مطالعه",
    views: "۱۸.۲K بازدید",
  },
  {
    id: "4",
    title: "Kubernetes 1.33 tightens supply-chain defaults",
    tag: "DevOps",
    summary:
      "Signed artifacts and stricter admission policies become easier to enable without custom controllers.",
    time: "5h ago",
    image: "/home/icon-devops.png",
    readTime: "۵ دقیقه مطالعه",
    views: "۸.۱K بازدید",
  },
  {
    id: "5",
    title: "Server Components patterns that survive scale",
    tag: "React",
    summary:
      "Teams share caching boundaries, streaming layouts, and data-loading rules that hold up under real traffic.",
    time: "Yesterday",
    image: "/home/icon-code.png",
    readTime: "۸ دقیقه مطالعه",
    views: "۱۱.۰K بازدید",
  },
  {
    id: "6",
    title: "C# 14 proposals aim at everyday productivity",
    tag: ".NET",
    summary:
      "Smaller language improvements target null handling, collection expressions, and clearer diagnostics.",
    time: "Yesterday",
    image: "/home/icon-backend.png",
    readTime: "۴ دقیقه مطالعه",
    views: "۷.۶K بازدید",
  },
  {
    id: "7",
    title: "Prompt evaluation becomes standard CI practice",
    tag: "AI",
    summary:
      "Engineering teams treat prompt regressions like unit tests, with fixtures and score thresholds in pipelines.",
    time: "Yesterday",
    image: "/home/icon-prompt.png",
    readTime: "۶ دقیقه مطالعه",
    views: "۱۵.۳K بازدید",
  },
  {
    id: "8",
    title: "Platform teams standardize on OpenTelemetry",
    tag: "DevOps",
    summary:
      "Traces, metrics, and logs converge on one collector path across services written in different languages.",
    time: "2d ago",
    image: "/home/icon-scan.png",
    readTime: "۵ دقیقه مطالعه",
    views: "۶.۹K بازدید",
  },
  {
    id: "9",
    title: "React Native’s New Architecture adoption accelerates",
    tag: "React",
    summary:
      "Fabric and TurboModules become the default path for new apps, with migration guides for legacy modules.",
    time: "2d ago",
    image: "/home/icon-mobile.png",
    readTime: "۷ دقیقه مطالعه",
    views: "۱۰.۵K بازدید",
  },
  {
    id: "10",
    title: "Blazor United simplifies full-stack .NET UI",
    tag: ".NET",
    summary:
      "A single project model blends server and client rendering, reducing ceremony for internal business apps.",
    time: "3d ago",
    image: "/home/icon-architect.png",
    readTime: "۶ دقیقه مطالعه",
    views: "۵.۴K بازدید",
  },
  {
    id: "11",
    title: "Local LLMs get practical for private codebases",
    tag: "AI",
    summary:
      "Smaller open models and better tooling make on-device assistants viable for teams with strict data policies.",
    time: "3d ago",
    image: "/home/icon-security.png",
    readTime: "۹ دقیقه مطالعه",
    views: "۱۳.۷K بازدید",
  },
  {
    id: "12",
    title: "GitHub Actions cost controls get more granular",
    tag: "DevOps",
    summary:
      "Per-workflow budgets and idle runner limits help teams cut CI spend without slowing critical pipelines.",
    time: "4d ago",
    image: "/home/icon-tools.png",
    readTime: "۴ دقیقه مطالعه",
    views: "۴.۸K بازدید",
  },
];

function matchesCategory(article: NewsArticle, category: NewsCategoryId): boolean {
  if (category === "همه") return true;
  const hay = `${article.title} ${article.summary} ${article.tag}`.toLowerCase();
  switch (category) {
    case "AI":
      return article.tag === "AI";
    case "Programming":
      return article.tag === "React" || hay.includes("compiler") || hay.includes("c#");
    case ".NET":
      return article.tag === ".NET";
    case "Frontend":
      return article.tag === "React" || hay.includes("blazor") || hay.includes("ui");
    case "Backend":
      return article.tag === ".NET" || hay.includes("api") || hay.includes("server");
    case "DevOps":
      return article.tag === "DevOps";
    case "Tools":
      return (
        hay.includes("tool") ||
        hay.includes("github actions") ||
        hay.includes("opentelemetry") ||
        hay.includes("prompt") ||
        article.image.includes("tools") ||
        article.image.includes("prompt")
      );
    case "Security":
      return (
        hay.includes("security") ||
        hay.includes("supply-chain") ||
        hay.includes("private") ||
        hay.includes("signed") ||
        article.image.includes("security")
      );
    default:
      return true;
  }
}

export function filterNewsArticles(
  articles: NewsArticle[],
  category: NewsCategoryId,
  cloudTag: NewsCloudTag,
): NewsArticle[] {
  let next = articles.filter((article) => matchesCategory(article, category));
  if (cloudTag !== "همه") {
    const key = cloudTag.toLowerCase();
    next = next.filter((article) => {
      const hay = `${article.title} ${article.summary} ${article.tag}`.toLowerCase();
      if (key === "nextjs") return hay.includes("react") || hay.includes("server component");
      if (key === "docker") return hay.includes("kubernetes") || hay.includes("container") || hay.includes("devops");
      if (key === "cursor" || key === "claude" || key === "mcp") {
        return hay.includes("ai") || hay.includes("prompt") || hay.includes("llm") || hay.includes("agent");
      }
      return hay.includes(key);
    });
  }
  return next;
}

/** Short view label for popular list (e.g. ۱۲.۴K). */
export function formatNewsViewsShort(views: string): string {
  return views.replace(/\s*بازدید\s*$/u, "").trim();
}
