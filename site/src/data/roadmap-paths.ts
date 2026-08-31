export type RoadmapTrack = "Frontend" | "Backend" | "DevOps" | "Mobile" | "AI" | "Other";

export type RoadmapPathFilter = "همه" | RoadmapTrack;

export const ROADMAP_PATH_FILTERS: readonly {
  id: RoadmapPathFilter;
  label: string;
  icon: string;
}[] = [
  { id: "همه", label: "همه", icon: "all" },
  { id: "Frontend", label: "Frontend", icon: "frontend" },
  { id: "Backend", label: "Backend", icon: "backend" },
  { id: "DevOps", label: "DevOps", icon: "devops" },
  { id: "Mobile", label: "Mobile", icon: "mobile" },
  { id: "AI", label: "AI", icon: "ai" },
  { id: "Other", label: "سایر", icon: "other" },
];

export type RoadmapStage = {
  id: string;
  step: number;
  stepLabel: string;
  icon: "code" | "layout" | "server" | "infinity" | "rocket" | "trophy";
  title: string;
  description: string;
  progress: number;
  lessons: string;
  duration: string;
  tracks: RoadmapTrack[];
};

/** Stages run right-to-left in the reference: step 1 is the rightmost card. */
export const ROADMAP_STAGES: RoadmapStage[] = [
  {
    id: "basics",
    step: 1,
    stepLabel: "مرحله ۱",
    icon: "code",
    title: "مقدمات برنامه‌نویسی",
    description: "آشنایی با مفاهیم پایه برنامه‌نویسی، الگوریتم‌ها و ساختار داده‌ها",
    progress: 100,
    lessons: "۱۸ درس",
    duration: "۲۴ ساعت",
    tracks: ["Frontend", "Backend", "Mobile", "AI", "Other"],
  },
  {
    id: "frontend",
    step: 2,
    stepLabel: "مرحله ۲",
    icon: "layout",
    title: "توسعه وب Frontend",
    description: "HTML، CSS، JavaScript و کتابخانه‌های محبوب",
    progress: 75,
    lessons: "۲۴ درس",
    duration: "۳۶ ساعت",
    tracks: ["Frontend"],
  },
  {
    id: "backend",
    step: 3,
    stepLabel: "مرحله ۳",
    icon: "server",
    title: "توسعه وب Backend",
    description: "برنامه‌نویسی سمت سرور، پایگاه داده و ساخت API",
    progress: 45,
    lessons: "۲۲ درس",
    duration: "۴۰ ساعت",
    tracks: ["Backend"],
  },
  {
    id: "devops",
    step: 4,
    stepLabel: "مرحله ۴",
    icon: "infinity",
    title: "DevOps و زیرساخت",
    description: "Git، Docker، CI/CD و استقرار برنامه‌ها",
    progress: 30,
    lessons: "۱۶ درس",
    duration: "۲۸ ساعت",
    tracks: ["DevOps"],
  },
  {
    id: "projects",
    step: 5,
    stepLabel: "مرحله ۵",
    icon: "rocket",
    title: "پروژه‌های عملی",
    description: "ساخت پروژه‌های واقعی و تمرین مهارت‌های کسب‌شده",
    progress: 0,
    lessons: "۸ پروژه",
    duration: "—",
    tracks: ["Frontend", "Backend", "DevOps", "Mobile", "AI"],
  },
  {
    id: "growth",
    step: 6,
    stepLabel: "مرحله ۶",
    icon: "trophy",
    title: "تخصص و رشد",
    description: "انتخاب حوزه تخصصی و یادگیری پیشرفته در مسیر شغلی",
    progress: 0,
    lessons: "۵ مسیر",
    duration: "—",
    tracks: ["AI", "Other"],
  },
];

export const ROADMAP_STATS: readonly {
  id: string;
  value: string;
  label: string;
  icon: "users" | "clock" | "doc" | "map";
}[] = [
  { id: "devs", value: "۲۴K+", label: "توسعه‌دهنده همراه", icon: "users" },
  { id: "hours", value: "۴۰+", label: "ساعت آموزش", icon: "clock" },
  { id: "guides", value: "۱۲۰+", label: "راهنما و مقاله", icon: "doc" },
  { id: "paths", value: "۱۲+", label: "مسیر یادگیری", icon: "map" },
];

export function filterRoadmapStages(
  stages: RoadmapStage[],
  track: RoadmapPathFilter,
): RoadmapStage[] {
  if (track === "همه") return stages;
  return stages.filter((stage) => stage.tracks.includes(track));
}
