export const HOME_TRUST_ACCENTS = ["purple", "blue", "cyan"] as const;

export type HomeTrustAccent = (typeof HOME_TRUST_ACCENTS)[number];

export type HomeTrustMarkItem = {
  id: string;
  name: string;
  accent: HomeTrustAccent;
};

type HomeTrustMarkProps = {
  item: HomeTrustMarkItem;
};

/** Stack mark tile — local SVG only, never a customer logo or remote asset. */
export function HomeTrustMark({ item }: HomeTrustMarkProps) {
  return (
    <li className={`home-trust-mark home-trust-mark-${item.accent}`}>
      <span className="home-trust-icon" aria-hidden>
        <TrustIcon id={item.id} />
      </span>
      <span className="home-trust-name">{item.name}</span>
    </li>
  );
}

function TrustIcon({ id }: { id: string }) {
  const common = {
    width: 22,
    height: 22,
    viewBox: "0 0 24 24",
    fill: "none",
    stroke: "currentColor",
    strokeWidth: 1.6,
  } as const;

  if (id === "next") {
    return (
      <svg {...common}>
        <circle cx="12" cy="12" r="9" />
        <path d="M9 16V8l7 8V8" />
      </svg>
    );
  }

  if (id === "vercel") {
    return (
      <svg {...common} fill="currentColor" stroke="none">
        <path d="M12 4.5 21 20H3L12 4.5Z" />
      </svg>
    );
  }

  if (id === "dotnet") {
    return (
      <svg {...common}>
        <rect x="3.5" y="3.5" width="7" height="7" rx="1.4" />
        <rect x="13.5" y="3.5" width="7" height="7" rx="1.4" />
        <rect x="3.5" y="13.5" width="7" height="7" rx="1.4" />
        <rect x="13.5" y="13.5" width="7" height="7" rx="1.4" />
      </svg>
    );
  }

  if (id === "postgres") {
    return (
      <svg {...common}>
        <ellipse cx="12" cy="7" rx="7" ry="2.6" />
        <path d="M5 7v5c0 1.5 3.1 2.7 7 2.7s7-1.2 7-2.7V7" />
        <path d="M5 12v5c0 1.5 3.1 2.7 7 2.7s7-1.2 7-2.7v-5" />
      </svg>
    );
  }

  return (
    <svg {...common} fill="currentColor" stroke="none">
      <path d="M12 2C6.5 2 2 6.6 2 12.2c0 4.5 2.9 8.3 6.9 9.6.5.1.7-.2.7-.5v-1.7c-2.8.6-3.4-1.4-3.4-1.4-.5-1.1-1.1-1.4-1.1-1.4-.9-.6.1-.6.1-.6 1 .1 1.5 1 1.5 1 .9 1.6 2.4 1.1 3 .9.1-.7.4-1.1.6-1.4-2.2-.3-4.6-1.1-4.6-5 0-1.1.4-2 1-2.7-.1-.3-.4-1.3.1-2.7 0 0 .8-.3 2.8 1a9.4 9.4 0 0 1 5 0c2-1.3 2.8-1 2.8-1 .5 1.4.2 2.4.1 2.7.6.7 1 1.6 1 2.7 0 3.9-2.3 4.7-4.6 5 .4.3.7.9.7 1.9v2.8c0 .3.2.6.7.5 4-1.3 6.9-5.1 6.9-9.6C22 6.6 17.5 2 12 2Z" />
    </svg>
  );
}
