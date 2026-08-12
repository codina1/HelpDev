"use client";

import Link from "next/link";
import { useCallback, useEffect, useMemo, useState } from "react";
import { useAuth } from "@/components/auth";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";
import { AdminLoadingState } from "@/components/admin/feedback/admin-loading-state";
import { AdminPageHeader } from "@/components/admin/page/admin-page-header";
import {
  applyAiContentWorkflowDraft,
  draftAiContentWorkflow,
  getAiContentWorkflow,
  outlineAiContentWorkflow,
  researchAiContentWorkflow,
  seoAiContentWorkflow,
  type AiContentWorkflowSessionDto,
  type AiResearchResultDto,
  type ContentOutlineDto,
  type DraftSuggestionDto,
  type SeoOptimizationSuggestionDto,
} from "@/lib/api/content-workflows";
import { ADMIN_ROUTES } from "@/lib/admin/routes";

const STEPS = ["Idea", "Research", "Outline", "Draft", "SEO", "Review"] as const;

type WizardStep = (typeof STEPS)[number];

type ContentWorkflowWizardProps = {
  workflowId: string;
};

export function ContentWorkflowWizard({ workflowId }: ContentWorkflowWizardProps) {
  const { token } = useAuth();
  const [session, setSession] = useState<AiContentWorkflowSessionDto | null>(null);
  const [step, setStep] = useState<WizardStep>("Idea");
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<unknown>(null);
  const [research, setResearch] = useState<AiResearchResultDto | null>(null);
  const [outline, setOutline] = useState<ContentOutlineDto | null>(null);
  const [outlineText, setOutlineText] = useState("");
  const [draft, setDraft] = useState<DraftSuggestionDto | null>(null);
  const [draftTitle, setDraftTitle] = useState("");
  const [draftBody, setDraftBody] = useState("");
  const [seo, setSeo] = useState<SeoOptimizationSuggestionDto | null>(null);
  const [appliedContentId, setAppliedContentId] = useState<string | null>(null);

  const load = useCallback(() => {
    if (!token || !workflowId) return;
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    getAiContentWorkflow(token, workflowId, controller.signal)
      .then((dto) => {
        setSession(dto);
        setAppliedContentId(dto.linkedContentId);
        setLoading(false);
      })
      .catch((err) => {
        if (controller.signal.aborted) return;
        setError(err);
        setLoading(false);
      });
    return () => controller.abort();
  }, [token, workflowId]);

  useEffect(() => {
    return load();
  }, [load]);

  const stepIndex = useMemo(() => STEPS.indexOf(step), [step]);

  async function runResearch() {
    if (!token) return;
    setBusy(true);
    setError(null);
    try {
      const result = await researchAiContentWorkflow(token, workflowId);
      setResearch(result);
      setStep("Research");
      await refreshSession();
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  }

  async function runOutline() {
    if (!token) return;
    setBusy(true);
    setError(null);
    try {
      const result = await outlineAiContentWorkflow(token, workflowId, research?.summary);
      setOutline(result);
      setOutlineText(result.rawText);
      setDraftTitle(result.title);
      setStep("Outline");
      await refreshSession();
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  }

  async function runDraft() {
    if (!token) return;
    setBusy(true);
    setError(null);
    try {
      const result = await draftAiContentWorkflow(token, workflowId, {
        outlineTitle: outline?.title || draftTitle || session?.idea.title || "",
        outlineText: outlineText || outline?.rawText || "",
      });
      setDraft(result);
      setDraftTitle(result.title);
      setDraftBody(result.bodyMarkdown);
      setStep("Draft");
      await refreshSession();
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  }

  async function runSeo() {
    if (!token) return;
    setBusy(true);
    setError(null);
    try {
      const result = await seoAiContentWorkflow(token, workflowId, {
        title: draftTitle,
        body: draftBody,
      });
      setSeo(result);
      setStep("SEO");
      await refreshSession();
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  }

  async function applyDraft() {
    if (!token) return;
    setBusy(true);
    setError(null);
    try {
      const result = await applyAiContentWorkflowDraft(token, workflowId, {
        title: draftTitle,
        body: draftBody,
        targetType: session?.idea.targetType,
      });
      setAppliedContentId(result.contentId);
      setStep("Review");
      await refreshSession();
    } catch (err) {
      setError(err);
    } finally {
      setBusy(false);
    }
  }

  async function refreshSession() {
    if (!token) return;
    const dto = await getAiContentWorkflow(token, workflowId);
    setSession(dto);
    setAppliedContentId(dto.linkedContentId);
  }

  if (loading && !session) return <AdminLoadingState cards={2} rows={5} />;

  return (
    <div className="space-y-6">
      <AdminPageHeader
        title={session?.idea.title ?? "Ú¯Ø±Ø¯Ø´ Ú©Ø§Ø± AI"}
        description="Ù…Ø±Ø§Ø­Ù„ Ø±Ø§ Ø¨Ù‡â€ŒØµÙˆØ±Øª Ø¯Ø³ØªÛŒ Ø§Ø¬Ø±Ø§ Ú©Ù†ÛŒØ¯. Ù‡ÛŒÚ† Ø®Ø±ÙˆØ¬ÛŒâ€ŒØ§ÛŒ Ø®ÙˆØ¯Ú©Ø§Ø± Ø°Ø®ÛŒØ±Ù‡ ÛŒØ§ Ù…Ù†ØªØ´Ø± Ù†Ù…ÛŒâ€ŒØ´ÙˆØ¯ Ù…Ú¯Ø± Ø¨Ø§ Ø¯Ú©Ù…Ù‡ Ø§Ø¹Ù…Ø§Ù„."
      />

      <ol className="flex flex-wrap gap-2">
        {STEPS.map((name, index) => (
          <li key={name}>
            <button
              type="button"
              onClick={() => setStep(name)}
              className={`rounded-md border px-3 py-1.5 text-[12px] ${
                step === name
                  ? "border-[var(--adm-accent)] bg-[var(--adm-accent-soft)] font-semibold"
                  : "border-[var(--adm-border)] adm-subtle"
              }`}
            >
              {index + 1}. {name}
            </button>
          </li>
        ))}
      </ol>

      {error ? (
        <AdminErrorState error={error} title="Ø®Ø·Ø§ Ø¯Ø± Ú¯Ø±Ø¯Ø´ Ú©Ø§Ø±" onRetry={load} showHome={false} />
      ) : null}

      {step === "Idea" && session ? (
        <Panel title="Ø§ÛŒØ¯Ù‡">
          <p className="adm-text text-[14px] font-semibold">{session.idea.title}</p>
          <p className="adm-subtle mt-2 text-[13px] whitespace-pre-wrap">{session.idea.description || "â€”"}</p>
          <p className="adm-subtle mt-2 text-[12px]" dir="ltr">
            status={session.idea.status} Â· step={session.currentStep}
          </p>
          <button
            type="button"
            disabled={busy}
            onClick={runResearch}
            className="mt-4 rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50"
          >
            Ø´Ø±ÙˆØ¹ Ù¾Ú˜ÙˆÙ‡Ø´ (RAG)
          </button>
        </Panel>
      ) : null}

      {step === "Research" ? (
        <Panel title="Ù¾Ú˜ÙˆÙ‡Ø´">
          {!research ? (
            <button type="button" disabled={busy} onClick={runResearch} className="rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
              Ø§Ø¬Ø±Ø§ÛŒ Ù¾Ú˜ÙˆÙ‡Ø´
            </button>
          ) : (
            <>
              <pre className="whitespace-pre-wrap rounded-md border border-[var(--adm-border)] p-3 text-[12px]">
                {research.summary}
              </pre>
              <ul className="mt-3 space-y-2">
                {research.sources.map((source) => (
                  <li key={`${source.sourceType}-${source.url}`} className="text-[12px]">
                    <span className="font-semibold">{source.title}</span>
                    <span className="adm-subtle"> Â· {source.sourceType}</span>
                    <p className="adm-subtle mt-1">{source.snippet}</p>
                  </li>
                ))}
              </ul>
              <button type="button" disabled={busy} onClick={runOutline} className="mt-4 rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
                ØªÙˆÙ„ÛŒØ¯ Ø³Ø§Ø®ØªØ§Ø±
              </button>
            </>
          )}
        </Panel>
      ) : null}

      {step === "Outline" ? (
        <Panel title="Ø³Ø§Ø®ØªØ§Ø± (Ù‚Ø§Ø¨Ù„ ÙˆÛŒØ±Ø§ÛŒØ´)">
          {!outline && !outlineText ? (
            <button type="button" disabled={busy} onClick={runOutline} className="rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
              ØªÙˆÙ„ÛŒØ¯ Ø³Ø§Ø®ØªØ§Ø±
            </button>
          ) : (
            <>
              <textarea
                className="min-h-[220px] w-full rounded-md border border-[var(--adm-border)] bg-[var(--adm-surface)] p-3 text-[13px]"
                value={outlineText}
                onChange={(e) => setOutlineText(e.target.value)}
              />
              <button type="button" disabled={busy} onClick={runDraft} className="mt-4 rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
                ØªÙˆÙ„ÛŒØ¯ Ù¾ÛŒØ´â€ŒÙ†ÙˆÛŒØ³
              </button>
            </>
          )}
        </Panel>
      ) : null}

      {step === "Draft" ? (
        <Panel title="Ù¾ÛŒØ´â€ŒÙ†ÙˆÛŒØ³ (Ø§Ø¹Ù…Ø§Ù„ Ø§Ø®ØªÛŒØ§Ø±ÛŒ)">
          {!draft && !draftBody ? (
            <button type="button" disabled={busy} onClick={runDraft} className="rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
              ØªÙˆÙ„ÛŒØ¯ Ù¾ÛŒØ´â€ŒÙ†ÙˆÛŒØ³
            </button>
          ) : (
            <>
              <input
                className="mb-3 w-full rounded-md border border-[var(--adm-border)] px-3 py-2 text-[13px]"
                value={draftTitle}
                onChange={(e) => setDraftTitle(e.target.value)}
              />
              <textarea
                className="min-h-[280px] w-full rounded-md border border-[var(--adm-border)] bg-[var(--adm-surface)] p-3 text-[13px]"
                value={draftBody}
                onChange={(e) => setDraftBody(e.target.value)}
              />
              <div className="mt-4 flex flex-wrap gap-2">
                <button type="button" disabled={busy} onClick={runSeo} className="rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
                  Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ SEO
                </button>
                <button type="button" disabled={busy} onClick={applyDraft} className="rounded-md border border-[var(--adm-border)] px-4 py-2 text-[13px] disabled:opacity-50">
                  Ø§Ø¹Ù…Ø§Ù„ Ø¨Ù‡â€ŒØ¹Ù†ÙˆØ§Ù† Draft Ù…Ø­ØªÙˆØ§
                </button>
              </div>
              <p className="adm-subtle mt-2 text-[12px]">
                Ø§Ø¹Ù…Ø§Ù„ ÙÙ‚Ø· Content Draft + Revision Ù…ÛŒâ€ŒØ³Ø§Ø²Ø¯. Ø§Ù†ØªØ´Ø§Ø± Ø§Ø² Ú¯Ø±Ø¯Ø´â€ŒÚ©Ø§Ø± Ø¹Ø§Ø¯ÛŒ Ù…Ø­ØªÙˆØ§ Ø§Ø³Øª.
              </p>
            </>
          )}
        </Panel>
      ) : null}

      {step === "SEO" ? (
        <Panel title="Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ SEO (Ø¨Ø¯ÙˆÙ† Ø§Ù…ØªÛŒØ§Ø² Ø±ØªØ¨Ù‡)">
          {!seo ? (
            <button type="button" disabled={busy || !draftBody} onClick={runSeo} className="rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
              ØªÙˆÙ„ÛŒØ¯ Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ SEO
            </button>
          ) : (
            <>
              <p className="text-[13px]">
                <strong>Ø¹Ù†ÙˆØ§Ù† Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ÛŒ:</strong> {seo.suggestedTitle ?? "â€”"}
              </p>
              <p className="mt-2 text-[13px]">
                <strong>ØªÙˆØ¶ÛŒØ­ Ù¾ÛŒØ´Ù†Ù‡Ø§Ø¯ÛŒ:</strong> {seo.suggestedDescription ?? "â€”"}
              </p>
              <p className="mt-2 text-[12px]">Ú©Ù„ÛŒØ¯ÙˆØ§Ú˜Ù‡â€ŒÙ‡Ø§: {seo.keywordSuggestions.join(" Â· ") || "â€”"}</p>
              <ul className="mt-3 list-disc pe-5 text-[12px]">
                {seo.recommendations.map((item) => (
                  <li key={item}>{item}</li>
                ))}
              </ul>
              <button type="button" disabled={busy} onClick={() => setStep("Review")} className="mt-4 rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
                Ø±ÙØªÙ† Ø¨Ù‡ Ø¨Ø§Ø²Ø¨ÛŒÙ†ÛŒ
              </button>
            </>
          )}
        </Panel>
      ) : null}

      {step === "Review" ? (
        <Panel title="Ø¨Ø§Ø²Ø¨ÛŒÙ†ÛŒ Ø§Ù†Ø³Ø§Ù†ÛŒ">
          {appliedContentId ? (
            <>
              <p className="text-[13px]">Ù¾ÛŒØ´â€ŒÙ†ÙˆÛŒØ³ Ø§Ø¹Ù…Ø§Ù„ Ø´Ø¯. Ø§Ù†ØªØ´Ø§Ø± Ø§Ø² Ù…Ø³ÛŒØ± Ø¹Ø§Ø¯ÛŒ Ù…Ø­ØªÙˆØ§ Ø§Ù†Ø¬Ø§Ù… Ù…ÛŒâ€ŒØ´ÙˆØ¯.</p>
              <Link
                href={`${ADMIN_ROUTES.content}/${encodeURIComponent(appliedContentId)}/workflow`}
                className="mt-3 inline-block underline text-[13px]"
              >
                Ø¨Ø§Ø² Ú©Ø±Ø¯Ù† Ú¯Ø±Ø¯Ø´â€ŒÚ©Ø§Ø± Ù…Ø­ØªÙˆØ§
              </Link>
            </>
          ) : (
            <>
              <p className="text-[13px]">Ù‡Ù†ÙˆØ² Ù¾ÛŒØ´â€ŒÙ†ÙˆÛŒØ³ÛŒ Ø§Ø¹Ù…Ø§Ù„ Ù†Ø´Ø¯Ù‡ Ø§Ø³Øª.</p>
              <button type="button" disabled={busy || !draftBody} onClick={applyDraft} className="mt-3 rounded-md bg-[var(--adm-accent)] px-4 py-2 text-[13px] font-semibold text-white disabled:opacity-50">
                Ø§Ø¹Ù…Ø§Ù„ Draft
              </button>
            </>
          )}
        </Panel>
      ) : null}

      <p className="adm-subtle text-[11px]">Ù…Ø±Ø­Ù„Ù‡ ÙØ¹Ù„ÛŒ UI: {stepIndex + 1}/{STEPS.length}</p>
    </div>
  );
}

function Panel({ title, children }: { title: string; children: React.ReactNode }) {
  return (
    <section className="space-y-3 rounded-lg border border-[var(--adm-border)] bg-[var(--adm-surface)] p-4">
      <h2 className="adm-text text-[15px] font-bold">{title}</h2>
      {children}
    </section>
  );
}

