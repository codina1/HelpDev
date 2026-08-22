"use client";

import type { SlashCommandItem } from "@/lib/admin/content/block-editor/slash-items";
import styles from "./article-rich-text-editor.module.css";

type SlashCommandMenuProps = {
  items: SlashCommandItem[];
  activeIndex: number;
  left: number;
  top: number;
  onSelect: (item: SlashCommandItem) => void;
};

export function SlashCommandMenu({ items, activeIndex, left, top, onSelect }: SlashCommandMenuProps) {
  if (items.length === 0) return null;
  return (
    <div className={styles.slash} style={{ left, top }} role="listbox" aria-label="درج بلوک">
      {items.map((item, index) => (
        <button
          key={item.id}
          type="button"
          role="option"
          aria-selected={index === activeIndex}
          className={`${styles.slashItem} ${index === activeIndex ? styles.slashItemActive : ""}`}
          onMouseDown={(event) => {
            event.preventDefault();
            onSelect(item);
          }}
        >
          {item.title}
        </button>
      ))}
    </div>
  );
}
