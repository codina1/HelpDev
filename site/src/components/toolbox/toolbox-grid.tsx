import { ToolCard } from "@/components/toolbox/tool-card";
import type { ToolItem } from "@/types";

type ToolboxGridProps = {
  items: ToolItem[];
};

export function ToolboxGrid({ items }: ToolboxGridProps) {
  return (
    <div className="grid gap-4 sm:grid-cols-2">
      {items.map((item) => (
        <ToolCard key={item.id} item={item} />
      ))}
    </div>
  );
}
