"use client";

import { useId, useState } from "react";
import { InteractiveNode } from "@/components/experience/interactive-node";
import { KNOWLEDGE_CORE_NODES } from "@/lib/public/intelligence-showcase";

type KnowledgeGalaxyProps = {
  className?: string;
  onCenterActivate?: () => void;
};

/**
 * Animated AI Knowledge Core — cross layout with glow, pulsing edges, floating particles.
 */
export function KnowledgeGalaxy({ className = "", onCenterActivate }: KnowledgeGalaxyProps) {
  const [activeId, setActiveId] = useState<string | null>(null);
  const gradId = useId().replace(/:/g, "");
  const glowId = useId().replace(/:/g, "");

  return (
    <div
      className={["relative mx-auto aspect-square w-full max-w-[440px]", className].join(" ")}
      role="img"
      aria-label="هسته دانش AI — Articles، Tools، Roadmaps و Learning پیرامون HelpDev AI"
    >
      <div className="exp-galaxy-aura absolute inset-[4%] rounded-full" aria-hidden />
      <div className="exp-galaxy-ring absolute inset-[16%] rounded-full border border-[color:color-mix(in_srgb,var(--pub-secondary)_28%,transparent)]" aria-hidden />

      {/* Floating particles */}
      <span className="ix-particle absolute start-[18%] top-[20%] h-1.5 w-1.5 rounded-full bg-[color:var(--pub-primary)]" aria-hidden />
      <span className="ix-particle ix-particle-d1 absolute end-[22%] top-[18%] h-1 w-1 rounded-full bg-[color:var(--pub-secondary)]" aria-hidden />
      <span className="ix-particle ix-particle-d2 absolute start-[14%] bottom-[28%] h-1 w-1 rounded-full bg-[color:var(--pub-primary)]" aria-hidden />
      <span className="ix-particle ix-particle-d3 absolute end-[16%] bottom-[22%] h-1.5 w-1.5 rounded-full bg-[color:var(--pub-secondary)]" aria-hidden />
      <span className="ix-particle ix-particle-d1 absolute start-[42%] top-[8%] h-1 w-1 rounded-full bg-white/50" aria-hidden />
      <span className="ix-particle ix-particle-d2 absolute end-[40%] bottom-[10%] h-1 w-1 rounded-full bg-white/40" aria-hidden />

      <svg className="absolute inset-0 h-full w-full" viewBox="0 0 100 100" aria-hidden>
        <defs>
          <linearGradient id={gradId} x1="0%" y1="0%" x2="100%" y2="100%">
            <stop offset="0%" stopColor="var(--pub-primary)" />
            <stop offset="50%" stopColor="var(--pub-secondary)" />
            <stop offset="100%" stopColor="var(--pub-primary)" />
          </linearGradient>
          <filter id={glowId} x="-50%" y="-50%" width="200%" height="200%">
            <feGaussianBlur stdDeviation="1.4" result="blur" />
            <feMerge>
              <feMergeNode in="blur" />
              <feMergeNode in="SourceGraphic" />
            </feMerge>
          </filter>
        </defs>
        {KNOWLEDGE_CORE_NODES.map((node) => (
          <g key={node.id}>
            <line
              className={[
                "exp-edge",
                activeId && activeId !== node.id ? "opacity-20" : "opacity-95",
                activeId === node.id ? "exp-edge-active" : "",
              ].join(" ")}
              x1="50"
              y1="50"
              x2={node.x}
              y2={node.y}
              stroke={`url(#${gradId})`}
              strokeWidth={activeId === node.id ? 0.95 : 0.45}
              filter={`url(#${glowId})`}
            />
            <circle
              className="exp-edge-pulse"
              cx={(50 + node.x) / 2}
              cy={(50 + node.y) / 2}
              r="1"
              fill="var(--pub-secondary)"
              opacity={activeId === node.id ? 1 : 0.6}
            />
          </g>
        ))}
      </svg>

      <InteractiveNode
        label="HelpDev AI"
        description="هسته هوش مهندسی پلتفرم — Ask HelpDev AI"
        tone="center"
        size="lg"
        active={activeId === "center"}
        onActivate={onCenterActivate}
        onHoverChange={(h) => setActiveId(h ? "center" : null)}
        style={{ left: "50%", top: "50%" }}
        className="z-[2]"
      />

      {KNOWLEDGE_CORE_NODES.map((node) => (
        <div key={node.id} className={["absolute inset-0", node.float].join(" ")}>
          <InteractiveNode
            label={node.label}
            description={node.description}
            href={node.href}
            size="sm"
            active={activeId === node.id}
            onHoverChange={(h) => setActiveId(h ? node.id : null)}
            style={{ left: `${node.x}%`, top: `${node.y}%` }}
            className={activeId && activeId !== node.id ? "opacity-40" : ""}
          />
        </div>
      ))}
    </div>
  );
}
