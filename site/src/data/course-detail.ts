import { COURSES, formatCoursePrice, getCourseBySlug } from "@/data/courses";
import type { Course } from "@/types";

export type CourseDetailTabId =
  | "about"
  | "curriculum"
  | "instructor"
  | "reviews"
  | "projects"
  | "requirements";


export type CourseCurriculumLesson = {
  id: string;
  title: string;
  durationMinutes: number;
  isPreview?: boolean;
};

export type CourseCurriculumSection = {
  id: string;
  title: string;
  lessons: CourseCurriculumLesson[];
};

export type CourseReview = {
  id: string;
  author: string;
  rating: number;
  dateLabel: string;
  comment: string;
};

export type RelatedCourseCard = {
  id: string;
  slug: string;
  title: string;
  durationLabel: string;
  rating: number;
  image: string;
  priceLabel: string;
};

export type CourseDetailModel = {
  slug: string;
  category: string;
  title: string;
  titleAccent?: string;
  description: string;
  about: string;
  instructor: {
    name: string;
    role: string;
    bio: string;
    initials: string;
    avatarUrl?: string;
  };
  durationHours: number;
  sessionsCount: number;
  levelLabel: string;
  studentsCount: number;
  rating: number;
  price: number;
  originalPrice?: number;
  discountPercent?: number;
  previewImage: string;
  previewCaption: string;
  features: string[];
  highlights: { title: string; description: string; icon: "spark" | "briefcase" | "project" }[];
  learningOutcomes: string[];
  requirements: string[];
  curriculum: CourseCurriculumSection[];
  projects: string[];
  reviews: CourseReview[];
  related: RelatedCourseCard[];
  breadcrumb: { label: string; href?: string }[];
};

export const COURSE_DETAIL_TABS: { id: CourseDetailTabId; label: string }[] = [
  { id: "about", label: "معرفی دوره" },
  { id: "curriculum", label: "سرفصل‌ها" },
  { id: "instructor", label: "مدرس" },
  { id: "reviews", label: "نظرات" },
  { id: "projects", label: "پروژه‌ها" },
  { id: "requirements", label: "پیش‌نیازها" },
];

const REACT_19_COURSE: CourseDetailModel = {
  slug: "react-19",
  category: "Frontend",
  title: "دوره جامع React",
  titleAccent: "19",
  description:
    "از مفاهیم پایه تا ساخت پروژه‌های حرفه‌ای با React 19 و آماده ورود به بازار کار توسعه فرانت‌اند.",
  about:
    "در این دوره مسیر کامل React 19 را قدم‌به‌قدم طی می‌کنید؛ از مبانی کامپوننت و Hooks تا Server Components، مدیریت state و ساخت اپلیکیشن‌های واقعی. تمرکز روی تمرین عملی، استانداردهای تیم‌های حرفه‌ای و آمادگی برای موقعیت‌های شغلی Frontend است.",
  instructor: {
    name: "علیرضا محمدی",
    role: "Frontend Developer",
    bio: "توسعه‌دهنده فرانت‌اند با تجربه ساخت محصولات آموزشی و اپلیکیشن‌های مقیاس‌پذیر با React و Next.js.",
    initials: "عم",
  },
  durationHours: 18,
  sessionsCount: 65,
  levelLabel: "مقدماتی تا پیشرفته",
  studentsCount: 3800,
  rating: 4.9,
  price: 1290000,
  originalPrice: 1500000,
  discountPercent: 15,
  previewImage: "/courses/course-react.png",
  previewCaption: "Build Modern Web Apps · React 19",
  features: [
    "دسترسی دائمی",
    "آپدیت رایگان دوره",
    "پروژه عملی",
    "پشتیبانی مدرس",
    "فایل تمرین",
  ],
  highlights: [
    {
      title: "به‌روز و کامل",
      description: "پوشش قابلیت‌های جدید React 19 و الگوهای مدرن.",
      icon: "spark",
    },
    {
      title: "مناسب بازار کار",
      description: "تمرین روی سناریوهایی که در تیم‌های واقعی استفاده می‌شود.",
      icon: "briefcase",
    },
    {
      title: "پروژه محور",
      description: "ساخت چند پروژه کاربردی از صفر تا دیپلوی.",
      icon: "project",
    },
  ],
  learningOutcomes: [
    "مفاهیم جدید React 19",
    "Server Components",
    "Hooks پیشرفته",
    "State Management",
    "ساخت پروژه واقعی",
    "آماده شدن برای بازار کار",
  ],
  requirements: ["HTML", "CSS", "JavaScript", "مفاهیم پایه برنامه‌نویسی"],
  curriculum: [
    {
      id: "s1",
      title: "بخش ۱: مقدمات و آشنایی با React 19",
      lessons: [
        { id: "s1l1", title: "چرا React 19؟", durationMinutes: 18, isPreview: true },
        { id: "s1l2", title: "راه‌اندازی محیط توسعه", durationMinutes: 22 },
        { id: "s1l3", title: "اولین کامپوننت", durationMinutes: 25 },
        { id: "s1l4", title: "JSX و ساختار پروژه", durationMinutes: 30 },
      ],
    },
    {
      id: "s2",
      title: "بخش ۲: کامپوننت‌ها، Props و State",
      lessons: [
        { id: "s2l1", title: "Props و ترکیب کامپوننت‌ها", durationMinutes: 28 },
        { id: "s2l2", title: "useState و الگوهای رایج", durationMinutes: 32 },
        { id: "s2l3", title: "لیست‌ها و کلیدها", durationMinutes: 20 },
      ],
    },
    {
      id: "s3",
      title: "بخش ۳: Hooks و Side Effects",
      lessons: [
        { id: "s3l1", title: "useEffect درست و غلط", durationMinutes: 35 },
        { id: "s3l2", title: "useMemo و useCallback", durationMinutes: 30 },
        { id: "s3l3", title: "Custom Hooks", durationMinutes: 28 },
      ],
    },
    {
      id: "s4",
      title: "بخش ۴: Server Components و Data Fetching",
      lessons: [
        { id: "s4l1", title: "Server vs Client Components", durationMinutes: 34 },
        { id: "s4l2", title: "الگوهای واکشی داده", durationMinutes: 36 },
        { id: "s4l3", title: "Caching و Revalidation", durationMinutes: 30 },
      ],
    },
    {
      id: "s5",
      title: "بخش ۵: پروژه نهایی و آمادگی شغلی",
      lessons: [
        { id: "s5l1", title: "معماری پروژه واقعی", durationMinutes: 40 },
        { id: "s5l2", title: "تست و کیفیت کد", durationMinutes: 28 },
        { id: "s5l3", title: "پورتفولیو و مصاحبه", durationMinutes: 32 },
      ],
    },
  ],
  projects: [
    "داشبورد مدیریت محتوا با React 19",
    "اپ فروشگاهی با Server Components",
    "کلون UI یک Developer Platform",
  ],
  reviews: [
    {
      id: "r1",
      author: "سارا نوری",
      rating: 5,
      dateLabel: "۲ هفته پیش",
      comment: "بهترین دوره React که دیدم؛ پروژه‌ها واقعاً نزدیک به کار واقعی بودند.",
    },
    {
      id: "r2",
      author: "حسین کریمی",
      rating: 5,
      dateLabel: "۱ ماه پیش",
      comment: "توضیح Server Components عالی بود و پشتیبانی مدرس سریع پاسخ می‌داد.",
    },
    {
      id: "r3",
      author: "مینا کاظمی",
      rating: 4,
      dateLabel: "۱ ماه پیش",
      comment: "ساختار مرتب، تمرین‌های خوب و حس پیشرفت واضح در هر بخش.",
    },
  ],
  related: [
    {
      id: "rc1",
      slug: "nextjs-14",
      title: "Next.js 14 پیشرفته",
      durationLabel: "۱۶ ساعت",
      rating: 4.8,
      image: "/courses/course-node.png",
      priceLabel: "۱,۱۰۰,۰۰۰ تومان",
    },
    {
      id: "rc2",
      slug: "typescript",
      title: "TypeScript برای فرانت‌اند",
      durationLabel: "۱۲ ساعت",
      rating: 4.7,
      image: "/courses/course-dotnet.png",
      priceLabel: "۸۹۰,۰۰۰ تومان",
    },
    {
      id: "rc3",
      slug: "tailwind-css",
      title: "Tailwind CSS عملی",
      durationLabel: "۸ ساعت",
      rating: 4.6,
      image: "/courses/course-htmlcss.png",
      priceLabel: "۶۴۰,۰۰۰ تومان",
    },
  ],
  breadcrumb: [
    { label: "خانه", href: "/" },
    { label: "دوره‌ها", href: "/courses" },
    { label: "Frontend", href: "/courses" },
    { label: "React 19" },
  ],
};

const COURSE_BY_SLUG: Record<string, CourseDetailModel> = {
  "react-19": REACT_19_COURSE,
  react19: REACT_19_COURSE,
  "react-19-complete": REACT_19_COURSE,
};

function buildDetailFromCatalog(course: Course): CourseDetailModel {
  const related = COURSES.filter((item) => item.slug !== course.slug)
    .slice(0, 3)
    .map((item) => ({
      id: item.id,
      slug: item.slug,
      title: item.title,
      durationLabel: item.duration,
      rating: item.rating,
      image: item.image,
      priceLabel: formatCoursePrice(item.price),
    }));

  return {
    slug: course.slug,
    category: course.category,
    title: course.title,
    description: course.description,
    about: `${course.description} این دوره در مسیر یادگیری ${course.category} طراحی شده و شامل تمرین عملی، فایل‌های پروژه و پشتیبانی مدرس است.`,
    instructor: {
      name: "تیم HelpDev Academy",
      role: course.platform,
      bio: `مدرسین HelpDev با تمرکز روی ${course.category} و پروژه‌های واقعی.`,
      initials: "HD",
    },
    durationHours: course.durationHours,
    sessionsCount: Math.max(12, Math.round(course.durationHours * 2.5)),
    levelLabel: course.levelLabel,
    studentsCount: Math.max(400, Math.round(course.rating * 800)),
    rating: course.rating,
    price: course.price,
    previewImage: course.image,
    previewCaption: course.title,
    features: ["دسترسی دائمی", "آپدیت رایگان دوره", "پروژه عملی", "پشتیبانی مدرس"],
    highlights: [
      {
        title: "یادگیری کاربردی",
        description: "تمرین‌ها نزدیک به سناریوهای واقعی تیم‌های محصول.",
        icon: "spark",
      },
      {
        title: "پروژه محور",
        description: "خروجی قابل ارائه در پورتفولیو.",
        icon: "project",
      },
      {
        title: "آمادگی شغلی",
        description: "مهارت‌های مورد نیاز بازار کار در این حوزه.",
        icon: "briefcase",
      },
    ],
    learningOutcomes: [
      `درک مفاهیم کلیدی ${course.category}`,
      "پیاده‌سازی پروژه‌های عملی مرتبط",
      "آشنایی با ابزارها و استانداردهای روز",
      "آمادگی برای ادامه مسیر تخصصی",
    ],
    requirements: ["علاقه به یادگیری", "دسترسی به اینترنت", "آشنایی مقدماتی با رایانه"],
    curriculum: [
      {
        id: `${course.slug}-sec-1`,
        title: "شروع مسیر",
        lessons: [
          { id: `${course.slug}-l1`, title: "معرفی دوره و اهداف", durationMinutes: 12, isPreview: true },
          { id: `${course.slug}-l2`, title: "نصب ابزارها و آماده‌سازی محیط", durationMinutes: 18 },
          { id: `${course.slug}-l3`, title: "اولین تمرین عملی", durationMinutes: 22 },
        ],
      },
      {
        id: `${course.slug}-sec-2`,
        title: "مهارت‌های اصلی",
        lessons: [
          { id: `${course.slug}-l4`, title: "مفاهیم پایه", durationMinutes: 28 },
          { id: `${course.slug}-l5`, title: "الگوهای کاربردی", durationMinutes: 32 },
          { id: `${course.slug}-l6`, title: "پروژه میانی", durationMinutes: 40 },
        ],
      },
      {
        id: `${course.slug}-sec-3`,
        title: "پروژه نهایی",
        lessons: [
          { id: `${course.slug}-l7`, title: "طراحی و پیاده‌سازی", durationMinutes: 45 },
          { id: `${course.slug}-l8`, title: "بازبینی و انتشار", durationMinutes: 25 },
        ],
      },
    ],
    projects: ["پروژه تمرینی ابتدایی", "پروژه میانی", "پروژه نهایی قابل ارائه"],
    reviews: [
      {
        id: `${course.slug}-r1`,
        author: "کاربر HelpDev",
        rating: Math.max(4, Math.round(course.rating)),
        dateLabel: "۲ هفته پیش",
        comment: "محتوا مرتب بود و دقیقاً همان دوره‌ای بود که از روی کارت انتخاب کردم.",
      },
    ],
    related,
    breadcrumb: [
      { label: "خانه", href: "/" },
      { label: "دوره‌ها", href: "/courses" },
      { label: course.category, href: "/courses" },
      { label: course.title },
    ],
  };
}

export function getCourseDetailBySlug(slug: string): CourseDetailModel | null {
  const key = decodeURIComponent(slug).trim().toLowerCase();
  if (!key) return null;

  const rich = COURSE_BY_SLUG[key];
  if (rich) return rich;

  const catalog = getCourseBySlug(key);
  if (catalog) return buildDetailFromCatalog(catalog);

  return null;
}

export function formatToman(amount: number): string {
  return `${amount.toLocaleString("fa-IR")} تومان`;
}

export function formatStudents(count: number): string {
  if (count >= 1000) {
    const value = count / 1000;
    const rounded = value >= 10 ? Math.round(value) : Math.round(value * 10) / 10;
    return `${rounded.toLocaleString("fa-IR")}K`;
  }
  return count.toLocaleString("fa-IR");
}

export function sectionStats(section: CourseCurriculumSection): { lessons: number; hours: number } {
  const minutes = section.lessons.reduce((sum, lesson) => sum + lesson.durationMinutes, 0);
  return {
    lessons: section.lessons.length,
    hours: Math.max(1, Math.round(minutes / 60)),
  };
}
