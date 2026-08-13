export type HomeTrustMarkItem = {
  id: string;
  name: string;
  /** Local public asset only — never a remote logo URL. */
  src?: string;
};

type HomeTrustMarkProps = {
  item: HomeTrustMarkItem;
};

/** Monochrome brand placeholder — local asset or wordmark. */
export function HomeTrustMark({ item }: HomeTrustMarkProps) {
  const wordmarkAsset = item.id === "next";

  return (
    <li className="home-trust-mark">
      {item.src ? (
        <img src={item.src} alt="" className="home-trust-logo" />
      ) : null}
      <span className={wordmarkAsset ? "sr-only" : "home-trust-name"}>{item.name}</span>
    </li>
  );
}
