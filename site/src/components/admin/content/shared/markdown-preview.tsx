import { Fragment, type ReactNode } from "react";
import {
  parseInline,
  parseMarkdown,
  type InlineSegment,
} from "@/lib/admin/content/markdown";

/**
 * Safe Markdown preview. Renders a parsed block tree as React elements only —
 * never `dangerouslySetInnerHTML` — so text content is always escaped by React.
 */
export function MarkdownPreview({ source }: { source: string }) {
  const blocks = parseMarkdown(source);

  if (blocks.length === 0) {
    return <p className="adm-subtle text-[13px]">پیش‌نمایشی برای نمایش وجود ندارد.</p>;
  }

  return (
    <div className="space-y-3 text-[13px] leading-7">
      {blocks.map((block, index) => {
        switch (block.kind) {
          case "heading": {
            const cls =
              block.level === 1
                ? "adm-text text-lg font-black"
                : block.level === 2
                  ? "adm-text text-base font-bold"
                  : "adm-text text-[14px] font-bold";
            return (
              <p key={index} className={cls}>
                {renderInline(block.text)}
              </p>
            );
          }
          case "code":
            return (
              <pre
                key={index}
                dir="ltr"
                className="adm-scroll overflow-x-auto rounded-lg bg-[var(--adm-surface-3)] p-3 text-start text-[12px]"
              >
                <code className="font-mono text-[var(--adm-text)]">{block.text}</code>
              </pre>
            );
          case "list": {
            const ListTag = block.ordered ? "ol" : "ul";
            return (
              <ListTag
                key={index}
                className={`space-y-1 ps-5 ${block.ordered ? "list-decimal" : "list-disc"}`}
              >
                {block.items.map((item, itemIndex) => (
                  <li key={itemIndex} className="adm-text">
                    {renderInline(item)}
                  </li>
                ))}
              </ListTag>
            );
          }
          default:
            return (
              <p key={index} className="adm-text">
                {renderInline(block.text)}
              </p>
            );
        }
      })}
    </div>
  );
}

function renderInline(text: string): ReactNode {
  return parseInline(text).map((segment, index) => (
    <Fragment key={index}>{renderSegment(segment)}</Fragment>
  ));
}

function renderSegment(segment: InlineSegment): ReactNode {
  switch (segment.kind) {
    case "bold":
      return <strong className="font-bold">{segment.value}</strong>;
    case "italic":
      return <em className="italic">{segment.value}</em>;
    case "code":
      return (
        <code
          dir="ltr"
          className="rounded bg-[var(--adm-surface-3)] px-1 py-0.5 font-mono text-[12px]"
        >
          {segment.value}
        </code>
      );
    case "link":
      return (
        <a
          href={segment.href}
          target="_blank"
          rel="noopener noreferrer"
          className="text-[var(--adm-accent-text)] underline"
        >
          {segment.value}
        </a>
      );
    default:
      return segment.value;
  }
}
