import Link from "next/link";

type HomeWorkflowInputProps = {
  question: string;
  href?: string;
};

/** Decorative AI question field — visual mockup, not a live generator. */
export function HomeWorkflowInput({
  question,
  href = "/learning/assistant",
}: HomeWorkflowInputProps) {
  return (
    <Link
      href={href}
      className="home-workflow-input focus-ring"
      aria-label={`پرسش نمونه: ${question}`}
    >
      <span className="home-workflow-input-icon" aria-hidden>
        <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.8">
          <path d="M12 3l1.5 5.5L19 10l-5.5 1.5L12 17l-1.5-5.5L5 10l5.5-1.5L12 3z" />
        </svg>
      </span>
      <span className="home-workflow-input-text">{question}</span>
      <span className="home-workflow-input-action">بپرس</span>
    </Link>
  );
}
