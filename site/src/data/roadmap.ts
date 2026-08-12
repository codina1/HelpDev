import type { RoadmapStep } from "@/types";

export const FRONTEND_ROADMAP = {
  id: "frontend-developer",
  title: "Frontend Developer",
  description: "مسیر عملی از مبانی تا رابط کاربری آماده تولید.",
  steps: [
    {
      id: "html-css",
      title: "مبانی HTML و CSS",
      description: "نشانه‌گذاری معنایی، لایه‌بندی، فلکس و گرید.",
    },
    {
      id: "javascript",
      title: "اصول JavaScript",
      description: "DOM، کد ناهمگام، ماژول‌ها و سینتکس مدرن.",
    },
    {
      id: "responsive",
      title: "طراحی واکنش‌گرا",
      description: "موبایل‌فرست و الگوهای دسترس‌پذیر UI.",
    },
    {
      id: "git",
      title: "گیت و کنترل نسخه",
      description: "برنچینگ، پول‌ریکوئست و تاریخچه تمیز.",
    },
    {
      id: "react",
      title: "مبانی React",
      description: "کامپوننت، پراپس، استیت و افکت.",
    },
    {
      id: "typescript",
      title: "TypeScript",
      description: "تایپ‌ها، اینترفیس‌ها و API امن‌تر.",
    },
    {
      id: "nextjs",
      title: "Next.js",
      description: "App Router، روتینگ و سرور کامپوننت.",
    },
    {
      id: "state",
      title: "مدیریت وضعیت",
      description: "استیت محلی، کانتکست و زمان افزودن استور.",
    },
    {
      id: "testing",
      title: "تست‌نویسی",
      description: "یونیت، کامپوننت و تست انتهابه‌انتها.",
    },
    {
      id: "deploy",
      title: "استقرار",
      description: "انتشار در پروداکشن و مانیتورینگ ضروری.",
    },
  ] satisfies RoadmapStep[],
} as const;
