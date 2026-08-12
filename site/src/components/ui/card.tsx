type CardProps = {
  title: string;
  description?: string;
  meta?: string;
};

/** Legacy content card — now consumes design-system tokens. */
export function Card({ title, description, meta }: CardProps) {
  return (
    <article className="ds-surface ds-hover-lift p-5">
      <h3 className="text-[15px] font-bold text-[color:var(--ds-fg)]">{title}</h3>
      {description ? (
        <p className="mt-2 text-[13px] leading-6 text-[color:var(--ds-muted)]">{description}</p>
      ) : null}
      {meta ? <p className="mt-4 text-[11px] text-[color:var(--ds-muted)]">{meta}</p> : null}
    </article>
  );
}
