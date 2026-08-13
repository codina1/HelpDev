import Link from "next/link";

export const HOME_ARTICLE_TONES = ["purple", "cyan", "blue"] as const;

export type HomeArticleTone = (typeof HOME_ARTICLE_TONES)[number];

export type HomeArticleItem = {
  id: string;
  title: string;
  excerpt: string;
  href: string;
  category: string;
  readingTime: string;
  date: string;
  tone: HomeArticleTone;
  image: string;
};

type HomeArticleCardProps = {
  item: HomeArticleItem;
};

/** Latest-article card — category, visual, title, excerpt, reading time, date. */
export function HomeArticleCard({ item }: HomeArticleCardProps) {
  return (
    <li>
      <Link href={item.href} className={`home-article-card home-article-card-${item.tone} focus-ring`}>
        <div className="home-article-visual">
          <img src={item.image} alt="" className="home-article-image" />
          <span className="home-article-visual-shade" aria-hidden />
          <span className="home-article-category">{item.category}</span>
        </div>
        <div className="home-article-body">
          <h3 className="home-article-title">{item.title}</h3>
          <p className="home-article-excerpt">{item.excerpt}</p>
          <p className="home-article-meta">
            <span className="home-article-meta-item">
              <ClockIcon />
              {item.readingTime}
            </span>
            {item.date ? (
              <span className="home-article-meta-item">
                <CalendarIcon />
                {item.date}
              </span>
            ) : null}
          </p>
        </div>
      </Link>
    </li>
  );
}

function ClockIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <circle cx="12" cy="12" r="8.5" />
      <path d="M12 7.5V12l3 2" />
    </svg>
  );
}

function CalendarIcon() {
  return (
    <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8" aria-hidden>
      <rect x="4" y="5" width="16" height="15" rx="2" />
      <path d="M8 3.5V7M16 3.5V7M4 10h16" />
    </svg>
  );
}
