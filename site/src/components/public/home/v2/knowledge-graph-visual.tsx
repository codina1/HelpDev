import { HERO_KNOWLEDGE_NODES } from "@/lib/public/nav-v2";

/**
 * Decorative AI engineering knowledge graph — visualization only, no fake content.
 */
export function KnowledgeGraphVisual({ className = "" }: { className?: string }) {
  const nodes = HERO_KNOWLEDGE_NODES;
  const pairs: Array<[number, number]> = [
    [0, 1],
    [0, 2],
    [1, 3],
    [2, 3],
    [0, 3],
  ];

  return (
    <div
      className={["relative mx-auto aspect-square w-full max-w-[420px]", className].join(" ")}
      aria-hidden
    >
      <div className="absolute inset-6 rounded-full border border-[color:var(--pub-glass-border)] bg-[radial-gradient(circle_at_center,color-mix(in_srgb,var(--pub-primary)_12%,transparent),transparent_65%)]" />
      <svg className="absolute inset-0 h-full w-full" viewBox="0 0 100 100" role="presentation">
        {pairs.map(([a, b], i) => (
          <line
            key={i}
            x1={nodes[a].x}
            y1={nodes[a].y}
            x2={nodes[b].x}
            y2={nodes[b].y}
            stroke="url(#pub-edge)"
            strokeWidth="0.4"
            opacity="0.7"
          />
        ))}
        <defs>
          <linearGradient id="pub-edge" x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="var(--pub-primary)" />
            <stop offset="100%" stopColor="var(--pub-secondary)" />
          </linearGradient>
        </defs>
      </svg>
      {nodes.map((node, index) => (
        <div
          key={node.id}
          className={[
            "pub-node-pulse absolute flex -translate-x-1/2 -translate-y-1/2 flex-col items-center gap-1",
            index === 3 ? "[animation-delay:0.6s]" : "",
          ].join(" ")}
          style={{ left: `${node.x}%`, top: `${node.y}%` }}
        >
          <span
            className={[
              "flex h-11 w-11 items-center justify-center rounded-2xl border text-[10px] font-extrabold backdrop-blur-md sm:h-12 sm:w-12 sm:text-[11px]",
              node.id === "ai"
                ? "border-[color:color-mix(in_srgb,var(--pub-secondary)_50%,transparent)] bg-gradient-to-br from-[color:var(--pub-primary)] to-[color:var(--pub-secondary)] text-white shadow-[0_0_28px_var(--pub-glow)]"
                : "border-[color:var(--pub-glass-border)] bg-[color:var(--pub-glass-strong)] text-[color:var(--pub-fg)]",
            ].join(" ")}
          >
            {node.label.slice(0, 2)}
          </span>
          <span className="rounded-md bg-[color:var(--pub-bg)]/70 px-1.5 py-0.5 text-[10px] font-semibold text-[color:var(--pub-muted)] backdrop-blur">
            {node.label}
          </span>
        </div>
      ))}
    </div>
  );
}
