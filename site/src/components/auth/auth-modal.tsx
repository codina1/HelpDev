"use client";

import { useEffect, useRef, useState } from "react";
import { sendOtp, verifyOtp } from "@/lib/auth-api";
import { useAuth } from "@/components/auth/auth-provider";

type AuthModalProps = {
  open: boolean;
  onClose: () => void;
};

type Step = "mobile" | "otp";

export function AuthModal({ open, onClose }: AuthModalProps) {
  const { login } = useAuth();
  const [step, setStep] = useState<Step>("mobile");
  const [mobile, setMobile] = useState("");
  const [otp, setOtp] = useState("");
  const [devOtp, setDevOtp] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const mobileRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (!open) return;

    setStep("mobile");
    setMobile("");
    setOtp("");
    setDevOtp(null);
    setError(null);
    setLoading(false);

    const timer = window.setTimeout(() => mobileRef.current?.focus(), 50);
    return () => window.clearTimeout(timer);
  }, [open]);

  useEffect(() => {
    if (!open) return;

    function onKeyDown(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }

    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, [open, onClose]);

  if (!open) return null;

  async function handleSendOtp(event: React.FormEvent) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const response = await sendOtp(mobile.trim());
      setDevOtp(response.otp ?? null);
      setStep("otp");
    } catch (err) {
      setError(err instanceof Error ? err.message : "ارسال کد ناموفق بود.");
    } finally {
      setLoading(false);
    }
  }

  async function handleVerifyOtp(event: React.FormEvent) {
    event.preventDefault();
    setLoading(true);
    setError(null);

    try {
      const session = await verifyOtp(mobile.trim(), otp.trim());
      await login(session);
      onClose();
    } catch (err) {
      setError(err instanceof Error ? err.message : "تأیید کد ناموفق بود.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center p-4"
      role="dialog"
      aria-modal="true"
      aria-labelledby="auth-modal-title"
    >
      <button
        type="button"
        className="absolute inset-0 bg-black/70 backdrop-blur-sm"
        aria-label="بستن"
        onClick={onClose}
      />

      <div className="relative w-full max-w-md overflow-hidden rounded-2xl border border-white/10 bg-[#12101f] shadow-[0_24px_80px_rgba(0,0,0,0.55)]">
        <div className="border-b border-white/[0.06] bg-gradient-to-l from-violet-600/20 to-indigo-600/10 px-6 py-5">
          <h2 id="auth-modal-title" className="text-lg font-extrabold text-white">
            {step === "mobile" ? "ورود / ثبت‌نام" : "تأیید شماره موبایل"}
          </h2>
          <p className="mt-1 text-[13px] text-slate-400">
            {step === "mobile"
              ? "شماره موبایل خود را وارد کنید تا کد تأیید ارسال شود."
              : `کد ارسال‌شده به ${mobile} را وارد کنید.`}
          </p>
        </div>

        <div className="px-6 py-5">
          {step === "mobile" ? (
            <form onSubmit={handleSendOtp} className="space-y-4">
              <label className="block">
                <span className="mb-2 block text-[13px] font-semibold text-slate-300">
                  شماره موبایل
                </span>
                <input
                  ref={mobileRef}
                  type="tel"
                  inputMode="tel"
                  dir="ltr"
                  value={mobile}
                  onChange={(e) => setMobile(e.target.value)}
                  placeholder="09123456789"
                  className="focus-ring h-11 w-full rounded-xl border border-white/10 bg-white/[0.04] px-4 text-left text-[14px] text-white outline-none placeholder:text-slate-500"
                  required
                />
              </label>

              {error && <p className="text-[13px] text-red-400">{error}</p>}

              <button
                type="submit"
                disabled={loading}
                className="focus-ring h-11 w-full rounded-xl bg-gradient-to-l from-violet-600 to-indigo-600 text-[14px] font-bold text-white transition-opacity disabled:opacity-60"
              >
                {loading ? "در حال ارسال..." : "دریافت کد تأیید"}
              </button>
            </form>
          ) : (
            <form onSubmit={handleVerifyOtp} className="space-y-4">
              {devOtp && (
                <div className="rounded-xl border border-amber-500/30 bg-amber-500/10 px-4 py-3 text-[13px] text-amber-200">
                  کد تست (Development):{" "}
                  <span dir="ltr" className="font-mono font-bold">
                    {devOtp}
                  </span>
                </div>
              )}

              <label className="block">
                <span className="mb-2 block text-[13px] font-semibold text-slate-300">
                  کد ۶ رقمی
                </span>
                <input
                  type="text"
                  inputMode="numeric"
                  dir="ltr"
                  maxLength={6}
                  value={otp}
                  onChange={(e) => setOtp(e.target.value.replace(/\D/g, ""))}
                  placeholder="123456"
                  className="focus-ring h-11 w-full rounded-xl border border-white/10 bg-white/[0.04] px-4 text-center text-[18px] tracking-[0.35em] text-white outline-none placeholder:text-slate-500"
                  required
                  autoFocus
                />
              </label>

              {error && <p className="text-[13px] text-red-400">{error}</p>}

              <button
                type="submit"
                disabled={loading || otp.length !== 6}
                className="focus-ring h-11 w-full rounded-xl bg-gradient-to-l from-violet-600 to-indigo-600 text-[14px] font-bold text-white transition-opacity disabled:opacity-60"
              >
                {loading ? "در حال تأیید..." : "ورود"}
              </button>

              <button
                type="button"
                onClick={() => {
                  setStep("mobile");
                  setOtp("");
                  setDevOtp(null);
                  setError(null);
                }}
                className="focus-ring w-full text-[13px] font-semibold text-slate-400 transition-colors hover:text-white"
              >
                تغییر شماره موبایل
              </button>
            </form>
          )}
        </div>
      </div>
    </div>
  );
}
