export type RoadmapDetailTabId =
  | "intro"
  | "steps"
  | "projects"
  | "resources"
  | "faq"
  | "reviews";

export type RoadmapStepTopic = {
  id: string;
  title: string;
};

export type RoadmapTimelineStep = {
  id: string;
  order: number;
  title: string;
  description: string;
  lessonsLabel: string;
  durationLabel: string;
  progress: number;
  tone: string;
  topics: RoadmapStepTopic[];
  completed?: boolean;
};

export type RoadmapResourceItem = {
  id: string;
  title: string;
  description: string;
  href: string;
  icon: "vscode" | "node" | "git" | "figma" | "docs" | "react";
};

export type RoadmapRelatedItem = {
  id: string;
  slug: string;
  title: string;
  tech: string;
  tone: string;
};

export type RoadmapProjectItem = {
  id: string;
  title: string;
  description: string;
};

export type RoadmapFaqItem = {
  id: string;
  question: string;
  answer: string;
};

export type RoadmapReviewItem = {
  id: string;
  author: string;
  rating: number;
  comment: string;
};

export type RoadmapDetailModel = {
  slug: string;
  category: string;
  tech: string;
  levelLabel: string;
  title: string;
  description: string;
  about: string;
  durationLabel: string;
  stepsCount: number;
  resourcesCount: number;
  projectsCount: number;
  hoursLabel: string;
  updatedAtLabel: string;
  author: string;
  rating: number;
  studentsLabel: string;
  progressPercent: number;
  completedSteps: number;
  badges: string[];
  features: { id: string; title: string; description: string; icon: string }[];
  steps: RoadmapTimelineStep[];
  resources: RoadmapResourceItem[];
  related: RoadmapRelatedItem[];
  projects: RoadmapProjectItem[];
  faq: RoadmapFaqItem[];
  reviews: RoadmapReviewItem[];
  breadcrumb: { label: string; href?: string }[];
};

export const ROADMAP_DETAIL_TABS: { id: RoadmapDetailTabId; label: string }[] = [
  { id: "intro", label: "معرفی" },
  { id: "steps", label: "مراحل" },
  { id: "projects", label: "پروژه‌ها" },
  { id: "resources", label: "منابع" },
  { id: "faq", label: "سوالات متداول" },
  { id: "reviews", label: "نظرات" },
];

const REACT_STEPS: RoadmapTimelineStep[] = [
  {
    id: "s1",
    order: 1,
    title: "مبانی و پیش‌نیازها",
    description: "آماده‌سازی محیط توسعه، ابزارها و عادت‌های پایه‌ای قبل از شروع کدنویسی فرانت‌اند.",
    lessonsLabel: "۸ درس",
    durationLabel: "۱ هفته",
    progress: 100,
    tone: "bg-[#8B5CF6] shadow-[0_0_14px_rgba(139,92,246,0.55)]",
    completed: true,
    topics: [
      { id: "t1", title: "VS Code" },
      { id: "t2", title: "آشنایی با ابزارها" },
      { id: "t3", title: "Git پایه" },
      { id: "t4", title: "Prettier" },
    ],
  },
  {
    id: "s2",
    order: 2,
    title: "HTML و CSS",
    description: "ساخت صفحه معنایی، لایه‌بندی مدرن و استایل‌دهی ریسپانسیو.",
    lessonsLabel: "۱۴ درس",
    durationLabel: "۳ هفته",
    progress: 100,
    tone: "bg-[#EC4899] shadow-[0_0_14px_rgba(236,72,153,0.45)]",
    completed: true,
    topics: [
      { id: "t1", title: "Semantic HTML" },
      { id: "t2", title: "Flex & Grid" },
      { id: "t3", title: "Responsive" },
    ],
  },
  {
    id: "s3",
    order: 3,
    title: "JavaScript",
    description: "مبانی JS، DOM، asynchronous programming و الگوهای روزمره توسعه وب.",
    lessonsLabel: "۱۸ درس",
    durationLabel: "۴ هفته",
    progress: 80,
    tone: "bg-[#F59E0B] shadow-[0_0_14px_rgba(245,158,11,0.45)]",
    completed: true,
    topics: [
      { id: "t1", title: "ES6+" },
      { id: "t2", title: "Async/Await" },
      { id: "t3", title: "Modules" },
    ],
  },
  {
    id: "s4",
    order: 4,
    title: "TypeScript",
    description: "تایپ‌سیستم، interfaceها و آماده‌سازی پروژه برای مقیاس‌پذیری.",
    lessonsLabel: "۱۰ درس",
    durationLabel: "۲ هفته",
    progress: 40,
    tone: "bg-[#22C55E] shadow-[0_0_14px_rgba(34,197,94,0.45)]",
    completed: true,
    topics: [
      { id: "t1", title: "Types" },
      { id: "t2", title: "Generics" },
      { id: "t3", title: "Strict Mode" },
    ],
  },
  {
    id: "s5",
    order: 5,
    title: "React Basics",
    description: "کامپوننت‌ها، JSX، props، state و چرخه رندر.",
    lessonsLabel: "۱۶ درس",
    durationLabel: "۳ هفته",
    progress: 10,
    tone: "bg-[#14B8A6] shadow-[0_0_14px_rgba(20,184,166,0.45)]",
    topics: [
      { id: "t1", title: "Components" },
      { id: "t2", title: "Hooks" },
      { id: "t3", title: "Composition" },
    ],
  },
  {
    id: "s6",
    order: 6,
    title: "State Management",
    description: "مدیریت state محلی و سراسری با Context، Zustand یا Redux Toolkit.",
    lessonsLabel: "۱۲ درس",
    durationLabel: "۲ هفته",
    progress: 0,
    tone: "bg-[#3B82F6] shadow-[0_0_14px_rgba(59,130,246,0.45)]",
    topics: [
      { id: "t1", title: "Context" },
      { id: "t2", title: "Zustand" },
      { id: "t3", title: "Patterns" },
    ],
  },
  {
    id: "s7",
    order: 7,
    title: "API Integration",
    description: "ارتباط با REST/GraphQL، کش داده و مدیریت خطا.",
    lessonsLabel: "۱۰ درس",
    durationLabel: "۲ هفته",
    progress: 0,
    tone: "bg-[#6366F1] shadow-[0_0_14px_rgba(99,102,241,0.45)]",
    topics: [
      { id: "t1", title: "Fetch" },
      { id: "t2", title: "React Query" },
      { id: "t3", title: "Auth" },
    ],
  },
  {
    id: "s8",
    order: 8,
    title: "محیط و ابزارها",
    description: "Vite، ESLint، تست اولیه و استانداردهای تیم.",
    lessonsLabel: "۸ درس",
    durationLabel: "۱ هفته",
    progress: 0,
    tone: "bg-[#A855F7] shadow-[0_0_14px_rgba(168,85,247,0.45)]",
    topics: [
      { id: "t1", title: "Vite" },
      { id: "t2", title: "ESLint" },
      { id: "t3", title: "CI basics" },
    ],
  },
  {
    id: "s9",
    order: 9,
    title: "پروژه‌های عملی",
    description: "ساخت چند پروژه واقعی برای تثبیت مهارت‌ها.",
    lessonsLabel: "۶ پروژه",
    durationLabel: "۴ هفته",
    progress: 0,
    tone: "bg-[#F43F5E] shadow-[0_0_14px_rgba(244,63,94,0.45)]",
    topics: [
      { id: "t1", title: "Dashboard" },
      { id: "t2", title: "Marketplace" },
      { id: "t3", title: "Auth App" },
    ],
  },
  {
    id: "s10",
    order: 10,
    title: "موضوعات پیشرفته",
    description: "Performance، Server Components و الگوهای معماری فرانت‌اند.",
    lessonsLabel: "۱۲ درس",
    durationLabel: "۳ هفته",
    progress: 0,
    tone: "bg-[#0EA5E9] shadow-[0_0_14px_rgba(14,165,233,0.45)]",
    topics: [
      { id: "t1", title: "Performance" },
      { id: "t2", title: "RSC" },
      { id: "t3", title: "Architecture" },
    ],
  },
  {
    id: "s11",
    order: 11,
    title: "آمادگی شغلی",
    description: "پورتفولیو، مصاحبه فنی و مسیر ورود به بازار کار.",
    lessonsLabel: "۷ درس",
    durationLabel: "۲ هفته",
    progress: 0,
    tone: "bg-[#8B5CF6] shadow-[0_0_14px_rgba(139,92,246,0.55)]",
    topics: [
      { id: "t1", title: "Portfolio" },
      { id: "t2", title: "Interview" },
      { id: "t3", title: "Soft skills" },
    ],
  },
];

export const REACT_ROADMAP_DETAIL: RoadmapDetailModel = {
  slug: "react-developer",
  category: "Frontend",
  tech: "React",
  levelLabel: "مبتدی تا پیشرفته",
  title: "نقشه راه کامل یادگیری React",
  description: "از مفاهیم پایه تا تسلط حرفه‌ای و ورود به بازار کار",
  about:
    "این نقشه راه یک مسیر استاندارد و مرحله‌به‌مرحله برای تبدیل شدن به توسعه‌دهنده React است. از مبانی وب تا پروژه‌های واقعی، منابع معتبر و آمادگی شغلی را پوشش می‌دهد تا بدون سردرگمی پیشرفت کنید.",
  durationLabel: "۶ ماه",
  stepsCount: 11,
  resourcesCount: 120,
  projectsCount: 9,
  hoursLabel: "۱۸۰+ ساعت",
  updatedAtLabel: "۱۴۰۴/۱۰/۱۵",
  author: "HelpDev",
  rating: 4.9,
  studentsLabel: "۱۲K+",
  progressPercent: 26,
  completedSteps: 4,
  badges: ["Frontend", "React", "مناسب مبتدی تا پیشرفته"],
  features: [
    {
      id: "f1",
      title: "مسیر استاندارد",
      description: "ترتیب یادگیری تأییدشده توسط تیم‌های محصول واقعی",
      icon: "path",
    },
    {
      id: "f2",
      title: "پروژه‌های عملی",
      description: "تمرین روی سناریوهای نزدیک به بازار کار",
      icon: "project",
    },
    {
      id: "f3",
      title: "منابع معتبر",
      description: "انتخاب منابع کوتاه، به‌روز و قابل اتکا",
      icon: "book",
    },
    {
      id: "f4",
      title: "آمادگی شغلی",
      description: "پورتفولیو، مصاحبه و مهارت‌های نرم",
      icon: "job",
    },
  ],
  steps: REACT_STEPS,
  resources: [
    {
      id: "r1",
      title: "VS Code",
      description: "ویرایشگر اصلی توسعه فرانت‌اند",
      href: "https://code.visualstudio.com",
      icon: "vscode",
    },
    {
      id: "r2",
      title: "Node.js",
      description: "اجرای ابزارها و پکیج‌منیجر",
      href: "https://nodejs.org",
      icon: "node",
    },
    {
      id: "r3",
      title: "Git & GitHub",
      description: "نسخه‌بندی و همکاری تیمی",
      href: "https://github.com",
      icon: "git",
    },
    {
      id: "r4",
      title: "Figma",
      description: "خواندن و پیاده‌سازی طراحی UI",
      href: "https://figma.com",
      icon: "figma",
    },
    {
      id: "r5",
      title: "React Docs",
      description: "مستندات رسمی React",
      href: "https://react.dev",
      icon: "react",
    },
  ],
  related: [
    {
      id: "rel1",
      slug: "nextjs-developer",
      title: "Next.js Developer",
      tech: "Next.js",
      tone: "from-[#111827] to-[#2563EB]/30",
    },
    {
      id: "rel2",
      slug: "typescript-mastery",
      title: "TypeScript Mastery",
      tech: "TypeScript",
      tone: "from-[#0F172A] to-[#3178C6]/35",
    },
    {
      id: "rel3",
      slug: "javascript-mastery",
      title: "JavaScript Mastery",
      tech: "JavaScript",
      tone: "from-[#111827] to-[#F59E0B]/30",
    },
    {
      id: "rel4",
      slug: "nodejs-backend",
      title: "Node.js Backend",
      tech: "Node.js",
      tone: "from-[#052e16] to-[#22C55E]/25",
    },
  ],
  projects: [
    {
      id: "p1",
      title: "Dashboard تحلیلی",
      description: "ساخت پنل مدیریت با نمودار، فیلتر و state پیچیده",
    },
    {
      id: "p2",
      title: "Marketplace سبک",
      description: "لیست محصولات، سبد خرید و اتصال API",
    },
    {
      id: "p3",
      title: "Auth App",
      description: "ورود، نقش‌ها و محافظت مسیرها",
    },
  ],
  faq: [
    {
      id: "q1",
      question: "آیا این مسیر برای مبتدی مناسب است؟",
      answer: "بله. از مبانی وب شروع می‌شود و به‌تدریج به React پیشرفته می‌رسد.",
    },
    {
      id: "q2",
      question: "چقدر زمان لازم است؟",
      answer: "با dedicating منظم حدود ۶ ماه؛ می‌توانید سریع‌تر یا کندتر پیش بروید.",
    },
  ],
  reviews: [
    {
      id: "rv1",
      author: "سارا محمدی",
      rating: 5,
      comment: "مسیر شفاف بود و پروژه‌ها واقعاً به پورتفولیو کمک کرد.",
    },
    {
      id: "rv2",
      author: "علی رضایی",
      rating: 5,
      comment: "بهترین ترتیب یادگیری React که تا الان دیدم.",
    },
  ],
  breadcrumb: [
    { label: "خانه", href: "/" },
    { label: "Roadmap", href: "/roadmap" },
    { label: "Frontend", href: "/roadmap" },
    { label: "نقشه راه یادگیری React" },
  ],
};

const CATALOG: Record<string, RoadmapDetailModel> = {
  "react-developer": REACT_ROADMAP_DETAIL,
  react: REACT_ROADMAP_DETAIL,
  "frontend-react": REACT_ROADMAP_DETAIL,
  "frontend-path": REACT_ROADMAP_DETAIL,
};

function cloneRelated(base: RoadmapDetailModel, slug: string, title: string, tech: string): RoadmapDetailModel {
  return {
    ...base,
    slug,
    tech,
    title,
    description: `مسیر یادگیری تخصصی ${tech} برای توسعه‌دهندگان`,
    badges: [base.category, tech, base.levelLabel],
    breadcrumb: [
      { label: "خانه", href: "/" },
      { label: "Roadmap", href: "/roadmap" },
      { label: base.category, href: "/roadmap" },
      { label: title },
    ],
  };
}

for (const item of REACT_ROADMAP_DETAIL.related) {
  if (!CATALOG[item.slug]) {
    CATALOG[item.slug] = cloneRelated(REACT_ROADMAP_DETAIL, item.slug, item.title, item.tech);
  }
}

export function getRoadmapDetail(slug: string): RoadmapDetailModel | null {
  return CATALOG[slug] ?? null;
}

export function listRoadmapDetailSlugs(): string[] {
  return Object.keys(CATALOG);
}
