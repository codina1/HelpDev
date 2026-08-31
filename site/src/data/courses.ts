import type { Course, CourseCategory } from "@/types";

export const COURSE_CATEGORIES: CourseCategory[] = [
  "Programming",
  "Frontend",
  "Backend",
  "DevOps",
  "AI",
  "Tools",
  "Database",
  "Mobile",
];

export type CourseCategoryFilter = "همه" | CourseCategory;

export const COURSE_CATEGORY_FILTERS: readonly {
  id: CourseCategoryFilter;
  label: string;
  icon: string;
}[] = [
  { id: "همه", label: "همه", icon: "all" },
  { id: "Programming", label: "برنامه‌نویسی", icon: "code" },
  { id: "Frontend", label: "فرانت‌اند", icon: "frontend" },
  { id: "Backend", label: "بک‌اند", icon: "backend" },
  { id: "DevOps", label: "DevOps", icon: "devops" },
  { id: "AI", label: "هوش مصنوعی", icon: "ai" },
  { id: "Tools", label: "ابزارها", icon: "tools" },
  { id: "Database", label: "پایگاه داده", icon: "database" },
  { id: "Mobile", label: "موبایل", icon: "mobile" },
];

export const COURSE_LEVEL_FILTERS: readonly {
  id: "all" | Course["level"];
  label: string;
}[] = [
  { id: "all", label: "همه سطوح" },
  { id: "Beginner", label: "مبتدی" },
  { id: "Intermediate", label: "متوسط" },
  { id: "Advanced", label: "پیشرفته" },
];

export type CoursePriceFilter = "all" | "free" | "paid" | "featured";

export const COURSE_PRICE_FILTERS: readonly { id: CoursePriceFilter; label: string }[] = [
  { id: "all", label: "همه" },
  { id: "free", label: "رایگان" },
  { id: "paid", label: "پولی" },
  { id: "featured", label: "ویژه" },
];

/** Catalog order follows the reference screenshot (RTL: first item is rightmost). */
export const COURSES: Course[] = [
  {
    id: "1",
    title: "Python برای همه (مقدماتی تا پیشرفته)",
    description: "یادگیری Python از مبانی تا ساخت پروژه‌های واقعی.",
    level: "Beginner",
    levelLabel: "مبتدی",
    platform: "HelpDev Academy",
    rating: 4.8,
    category: "Programming",
    categories: ["Programming", "AI"],
    image: "/courses/course-python.png",
    duration: "۲۰ ساعت",
    durationHours: 20,
    price: 890000,
  },
  {
    id: "2",
    title: "ASP.NET Core Web API به صورت عملی",
    description: "ساخت API های مقیاس‌پذیر و امن با ASP.NET Core.",
    level: "Advanced",
    levelLabel: "پیشرفته",
    platform: "HelpDev Academy",
    rating: 4.7,
    category: "Backend",
    categories: ["Backend", "Programming", "Database"],
    image: "/courses/course-dotnet.png",
    duration: "۳۰ ساعت",
    durationHours: 30,
    price: 990000,
  },
  {
    id: "3",
    title: "آموزش React از مقدماتی تا پیشرفته",
    description: "ساخت پروژه‌های حرفه‌ای با React و مدیریت state با Hooks.",
    level: "Intermediate",
    levelLabel: "متوسط",
    platform: "HelpDev Academy",
    rating: 4.9,
    category: "Frontend",
    categories: ["Frontend", "Programming"],
    image: "/courses/course-react.png",
    duration: "۲۶ ساعت",
    durationHours: 26,
    price: 990000,
  },
  {
    id: "4",
    title: "آموزش جامع HTML, CSS از صفر تا حرفه‌ای",
    description: "ساختار صفحات وب و استایل‌دهی حرفه‌ای را یاد بگیرید.",
    level: "Beginner",
    levelLabel: "مبتدی",
    platform: "HelpDev Academy",
    rating: 4.6,
    category: "Frontend",
    categories: ["Frontend", "Programming"],
    image: "/courses/course-htmlcss.png",
    duration: "۱۸ ساعت",
    durationHours: 18,
    price: 0,
  },
  {
    id: "5",
    title: "هوش مصنوعی برای توسعه‌دهندگان (پروژه محور)",
    description: "استفاده از AI در کدنویسی و ساخت ابزارهای هوشمند.",
    level: "Intermediate",
    levelLabel: "جدید",
    platform: "HelpDev Academy",
    rating: 4.9,
    category: "AI",
    categories: ["AI", "Tools", "Programming"],
    image: "/courses/course-ai.png",
    duration: "۱۴ ساعت",
    durationHours: 14,
    price: 1100000,
    isNew: true,
  },
  {
    id: "6",
    title: "Git و GitHub به زبان ساده",
    description: "مدیریت نسخه و همکاری تیمی با Git و GitHub.",
    level: "Beginner",
    levelLabel: "مبتدی",
    platform: "HelpDev Academy",
    rating: 4.7,
    category: "Tools",
    categories: ["Tools", "DevOps"],
    image: "/courses/course-git.png",
    duration: "۸ ساعت",
    durationHours: 8,
    price: 0,
  },
  {
    id: "7",
    title: "Docker و Kubernetes از صفر تا استقرار",
    description: "کانتینرسازی و استقرار برنامه‌ها با Docker و Kubernetes.",
    level: "Intermediate",
    levelLabel: "متوسط",
    platform: "HelpDev Academy",
    rating: 4.8,
    category: "DevOps",
    categories: ["DevOps", "Tools"],
    image: "/courses/course-docker.png",
    duration: "۲۲ ساعت",
    durationHours: 22,
    price: 960000,
  },
  {
    id: "8",
    title: "Node.js و Express.js برای توسعه‌دهندگان",
    description: "ساخت سرویس‌های سریع و مقیاس‌پذیر با Node و Express.",
    level: "Intermediate",
    levelLabel: "متوسط",
    platform: "HelpDev Academy",
    rating: 4.6,
    category: "Backend",
    categories: ["Backend", "Programming", "Database"],
    image: "/courses/course-node.png",
    duration: "۲۴ ساعت",
    durationHours: 24,
    price: 610000,
  },
];

/** Reference shows a catalog of 48 courses across 6 pages. */
export const COURSES_TOTAL_COUNT = 48;
export const COURSES_TOTAL_PAGES = 6;

export function formatCoursePrice(price: number): string {
  if (price === 0) return "رایگان";
  return `${price.toLocaleString("en-US")} تومان`;
}
