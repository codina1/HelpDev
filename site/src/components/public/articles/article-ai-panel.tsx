"use client";

import { useState } from "react";
import Link from "next/link";
import { Badge } from "@/components/ui/public/badge";
import { searchAsk } from "@/lib/api/search";

type ArticleAiAssistantPanelProps = {
  title: string;
  slug: string;
};

/**
 * AI assistant panel foundation on article detail.
 * Uses existing POST /search/ask when the user asks a question.
 */
export function ArticleAiAssistantPanel({ title, slug }: ArticleAiAssistantPanelProps) {
  const [question, setQuestion] = useState("");
  const [answer, setAnswer] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function onAsk(event: React.FormEvent) {
    event.preventDefault();
    const q = question.trim();
    if (!q) return;
    setLoading(true);
    setError(null);
    setAnswer(null);
    try {
      const result = await searchAsk(`درباره «${title}» (${slug}): ${q}`);
      setAnswer(result.answer);
    } catch (err) {
      setError(err instanceof Error ? err.message : "پاسخ دریافت نشد.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <aside
      className="rounded-2xl border border-[color:var(--border-strong)] bg-gradient-to-b from-violet-950/40 to-[color:var(--surface)] p-4"
      aria-labelledby="article-ai-title"
    >
      <div className="mb-3 flex items-center gap-2">
        <h2 id="article-ai-title" className="text-[13px] font-bold text-[color:var(--foreground)]">
          دستیار مقاله
        </h2>
        <Badge variant="ai">AI</Badge>
      </div>
      <p className="mb-3 text-[12px] leading-6 text-[color:var(--muted)]">
        سؤال خود را بپرسید — پاسخ از Search Ask API (RAG foundation).
      </p>
      <form onSubmit={onAsk} className="space-y-2">
        <label className="sr-only" htmlFor="article-ai-q">
          سؤال درباره این مقاله
        </label>
        <textarea
          id="article-ai-q"
          value={question}
          onChange={(e) => setQuestion(e.target.value)}
          rows={3}
          placeholder="مثلاً: خلاصه این مقاله چیست؟"
          className="focus-ring w-full resize-y rounded-xl border border-[color:var(--border-strong)] bg-[color:var(--surface-elevated)] px-3 py-2 text-[13px] text-[color:var(--foreground)] placeholder:text-[color:var(--muted)]"
        />
        <button
          type="submit"
          disabled={loading || !question.trim()}
          className="focus-ring w-full rounded-xl bg-gradient-to-l from-[color:var(--accent)] to-[color:var(--accent-2)] px-3 py-2 text-[12px] font-bold text-white disabled:opacity-50"
        >
          {loading ? "در حال پاسخ..." : "پرسیدن"}
        </button>
      </form>
      {error ? (
        <p className="mt-3 text-[12px] text-red-300" role="alert">
          {error}
        </p>
      ) : null}
      {answer ? (
        <div className="mt-3 rounded-xl border border-[color:var(--border)] bg-[color:var(--surface-elevated)] p-3 text-[13px] leading-7 text-[color:var(--foreground)]" role="status">
          {answer}
        </div>
      ) : null}
      <Link
        href="/learning/assistant"
        className="focus-ring mt-3 inline-flex text-[12px] font-semibold text-violet-300 hover:text-violet-200"
      >
        دستیار یادگیری کامل ←
      </Link>
    </aside>
  );
}
