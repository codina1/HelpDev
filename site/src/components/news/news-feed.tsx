import { NewsCard } from "@/components/news/news-card";
import type { NewsItem } from "@/types";

type NewsFeedProps = {
  items: NewsItem[];
};

export function NewsFeed({ items }: NewsFeedProps) {
  return (
    <div className="space-y-4">
      {items.map((item) => (
        <NewsCard key={item.id} item={item} />
      ))}
    </div>
  );
}
