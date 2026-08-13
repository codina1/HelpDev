export type HomeWorkflowStepItem = {
  id: string;
  number: number;
  title: string;
  caption: string;
};

type HomeWorkflowStepProps = {
  item: HomeWorkflowStepItem;
  state?: "upcoming" | "active" | "done";
};

const NUMBER_FA = new Intl.NumberFormat("fa-IR", { useGrouping: false });

/** Numbered workflow node with supporting labels. */
export function HomeWorkflowStep({ item, state = "upcoming" }: HomeWorkflowStepProps) {
  const active = state === "active";

  return (
    <li
      className="home-workflow-step"
      aria-current={active ? "step" : undefined}
    >
      <span
        className={[
          "home-workflow-node",
          active ? "home-workflow-node-active" : "",
          state === "done" ? "home-workflow-node-done" : "",
        ]
          .filter(Boolean)
          .join(" ")}
        aria-hidden
      >
        {NUMBER_FA.format(item.number)}
      </span>
      <div className="home-workflow-copy">
        <p className="home-workflow-title">{item.title}</p>
        <p className="home-workflow-caption">{item.caption}</p>
      </div>
    </li>
  );
}
