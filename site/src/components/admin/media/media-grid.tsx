import { MediaCard } from "@/components/admin/media/media-card";
import type { AdminMediaListItem } from "@/lib/admin/media/media-types";

type MediaGridProps = {
  items: AdminMediaListItem[];
  onItemClick: (item: AdminMediaListItem) => void;
  actionLabel?: string;
};

/** Responsive grid of media thumbnails. */
export function MediaGrid({ items, onItemClick, actionLabel }: MediaGridProps) {
  return (
    <div className="grid grid-cols-2 gap-3 sm:grid-cols-3 md:grid-cols-4 lg:grid-cols-6">
      {items.map((item) => (
        <MediaCard key={item.id} item={item} onClick={onItemClick} actionLabel={actionLabel} />
      ))}
    </div>
  );
}
