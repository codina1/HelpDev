/**
 * Sprint 50G — Premium Interaction Layer content (product narrative chrome).
 * Not a fake published catalog.
 */

export const AI_WORKFLOW_STEPS = [
  {
    id: "problem-understanding",
    code: "01",
    label: "Problem Understanding",
    titleFa: "درک مسئله",
    detail: "صورت‌مسئله، محدودیت‌ها و هدف را شفاف کنید",
  },
  {
    id: "architecture-analysis",
    code: "02",
    label: "Architecture Analysis",
    titleFa: "تحلیل معماری",
    detail: "گزینه‌ها، مرزها و الگوهای سیستم را بسنجید",
  },
  {
    id: "technology-decision",
    code: "03",
    label: "Technology Decision",
    titleFa: "تصمیم فناوری",
    detail: "استک و ابزار مناسب را انتخاب کنید",
  },
  {
    id: "implementation-roadmap",
    code: "04",
    label: "Implementation Roadmap",
    titleFa: "نقشه اجرای پیاده‌سازی",
    detail: "گام‌های اولویت‌دار و قابل‌اجرا بسازید",
  },
  {
    id: "engineering-solution",
    code: "05",
    label: "Engineering Solution",
    titleFa: "راه‌حل مهندسی",
    detail: "به دانش، ابزار و مسیر اجرا برسید",
  },
] as const;

export const AI_DECISION_DEMO_STEPS = AI_WORKFLOW_STEPS;

export const DEVELOPER_JOURNEY = [
  {
    id: "beginner",
    label: "Beginner",
    titleFa: "مبتدی",
    description: "مبانی مهندسی و اولین پروژه‌های ساخت‌یافته",
  },
  {
    id: "developer",
    label: "Developer",
    titleFa: "توسعه‌دهنده",
    description: "پیاده‌سازی محصول، ابزارها و گردش‌کار واقعی",
  },
  {
    id: "ai-engineer",
    label: "AI Engineer",
    titleFa: "مهندس AI",
    description: "ترکیب دانش، prompt و سیستم‌های هوشمند",
  },
  {
    id: "architect",
    label: "Architect",
    titleFa: "معمار",
    description: "تصمیم‌های معماری، مقیاس و پایداری سیستم",
  },
] as const;

export const INTELLIGENCE_CARDS = [
  {
    id: "knowledge",
    title: "دانش مهندسی",
    content: "معماری، الگوهای طراحی و تجربه پروژه‌های واقعی",
    accent: "primary" as const,
  },
  {
    id: "ai",
    title: "هوش مصنوعی",
    content: "تحلیل مسئله، تصمیم‌گیری و پیشنهاد راهکار",
    accent: "ai" as const,
  },
  {
    id: "memory",
    title: "حافظه مهندسی",
    content: "ثبت تصمیم‌ها و دانش پروژه‌ها",
    accent: "cyan" as const,
  },
] as const;

/** @deprecated Prefer INTELLIGENCE_CARDS — kept for soft compatibility. */
export const INTELLIGENCE_PILLARS = INTELLIGENCE_CARDS.map((c) => ({
  title: c.title,
  description: c.content,
}));

/** Engineering Stories — documentation-style methodology examples. */
export const ENGINEERING_STORIES = [
  {
    id: "netflix-scale",
    title: "Netflix Scale Architecture",
    challenge: "Handling millions of concurrent users",
    architecture: ["Microservices", "Event Driven", "Caching"],
    learning: "How scalable systems are designed",
    href: "/search?q=microservices%20architecture",
  },
  {
    id: "saas-multitenant",
    title: "SaaS Multi-Tenant Design",
    challenge: "Isolating tenants while sharing platform infrastructure",
    architecture: ["ASP.NET Core", "Tenant Boundaries", "Shared Kernel"],
    learning: "How multi-tenant SaaS boundaries stay clean at scale",
    href: "/search?q=SaaS%20multi-tenant%20ASP.NET",
  },
  {
    id: "ai-engineering-loop",
    title: "AI Engineering Decision Loop",
    challenge: "Turning vague product questions into executable engineering paths",
    architecture: ["Ask HelpDev AI", "Knowledge Retrieval", "Roadmap Generation"],
    learning: "How AI assists decisions without inventing catalog facts",
    href: "/learning/assistant",
  },
] as const;

/** @deprecated Prefer ENGINEERING_STORIES */
export const ENGINEERING_CASE_PATTERNS = ENGINEERING_STORIES.map((s) => ({
  id: s.id,
  domain: "Engineering Story",
  title: s.title,
  summary: s.challenge,
  href: s.href,
  tags: s.architecture,
}));

export const DEVELOPER_IDENTITY_OPTIONS = [
  {
    id: "beginner",
    label: "Beginner Developer",
    profile: "Junior Engineer",
    strength: "Foundations & Structured Learning",
    nextGrowth: ".NET Basics + Cloud Native",
  },
  {
    id: "software",
    label: "Software Engineer",
    profile: "Backend Developer",
    strength: ".NET Architecture",
    nextGrowth: "Cloud Native + AI Engineering",
  },
  {
    id: "ai",
    label: "AI Engineer",
    profile: "AI Systems Engineer",
    strength: "Prompting + Knowledge Systems",
    nextGrowth: "Architecture + Production AI Ops",
  },
  {
    id: "architect",
    label: "Engineering Architect",
    profile: "Systems Architect",
    strength: "Distributed Design Decisions",
    nextGrowth: "Platform Strategy + AI Governance",
  },
] as const;

export const KNOWLEDGE_CORE_NODES = [
  {
    id: "articles",
    label: "Articles",
    description: "AI-curated engineering knowledge",
    href: "/articles",
    x: 50,
    y: 12,
    float: "exp-float",
  },
  {
    id: "tools",
    label: "Tools",
    description: "Developer productivity tools",
    href: "/toolbox",
    x: 12,
    y: 50,
    float: "exp-float-d1",
  },
  {
    id: "roadmaps",
    label: "Roadmaps",
    description: "Personalized engineering paths",
    href: "/roadmap",
    x: 88,
    y: 50,
    float: "exp-float-d2",
  },
  {
    id: "learning",
    label: "Learning",
    description: "Structured skill growth",
    href: "/learning",
    x: 50,
    y: 88,
    float: "exp-float-d3",
  },
] as const;
