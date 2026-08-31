export type NavItem = {
  href: string;
  label: string;
};

export type TrendingItem = {
  id: string;
  title: string;
  meta: string;
};

export type NewsItem = {
  id: string;
  title: string;
  description: string;
  tag: string;
  time: string;
};

export type NewsTag = "React" | ".NET" | "AI" | "DevOps";

export type NewsArticle = {
  id: string;
  title: string;
  tag: NewsTag;
  categoryLabel?: string;
  summary: string;
  time: string;
  image: string;
  readTime: string;
  views: string;
};

export type RoadmapStep = {
  id: string;
  title: string;
  description: string;
};

export type ToolItem = {
  id: string;
  title: string;
  description: string;
  content: string;
};

export type CourseCategory =
  | "Programming"
  | "Frontend"
  | "Backend"
  | "DevOps"
  | "AI"
  | "Tools"
  | "Database"
  | "Mobile";

export type CourseLevel = "Beginner" | "Intermediate" | "Advanced";

export type Course = {
  id: string;
  title: string;
  description: string;
  level: CourseLevel;
  levelLabel: string;
  platform: string;
  rating: number;
  category: CourseCategory;
  categories: CourseCategory[];
  image: string;
  /** Persian duration label, e.g. «۱۲ ساعت». */
  duration: string;
  durationHours: number;
  /** 0 means free. */
  price: number;
  isNew?: boolean;
};

export type SearchTab = "news" | "roadmap" | "tools" | "courses";

export type SearchResult = {
  id: string;
  tab: SearchTab;
  title: string;
  summary: string;
  meta: string;
  href: string;
};
