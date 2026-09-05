"use client";

import { useState } from "react";
import { motion } from "framer-motion";

export function NewsletterCard({ compact = false }: { compact?: boolean }) {
  const [email, setEmail] = useState("");
  const [done, setDone] = useState(false);

  function onSubmit(event: React.FormEvent) {
    event.preventDefault();
    if (!email.trim()) return;
    setDone(true);
  }

  return (
    <aside
      className={[
        "overflow-hidden rounded-2xl border border-[rgba(139,92,246,0.35)] p-4 shadow-[0_0_28px_rgba(139,92,246,0.2)]",
        "bg-[linear-gradient(145deg,rgba(139,92,246,0.28),rgba(37,99,235,0.12)_45%,rgba(11,18,36,0.95))]",
      ].join(" ")}
    >
      <h2 className={`font-extrabold text-white ${compact ? "text-[13px]" : "text-[14.5px]"}`}>
        هیچ خبر مهمی را از دست ندهید
      </h2>
      <p className="mt-1.5 text-[12px] leading-6 text-[#CBD5E1]">
        خلاصه هفتگی اخبار AI و توسعه وب را در ایمیل بگیرید.
      </p>
      {done ? (
        <p className="mt-3 text-[12.5px] font-bold text-[#E9D5FF]">عضویت شما ثبت شد.</p>
      ) : (
        <form onSubmit={onSubmit} className="mt-3 space-y-2">
          <input
            type="email"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            placeholder="ایمیل شما"
            className="h-10 w-full rounded-xl border border-white/[0.1] bg-[#070B18]/85 px-3 text-[12.5px] text-white outline-none placeholder:text-[#64748B] focus:border-[#8B5CF6]/5"
          />
          <motion.button
            type="submit"
            whileTap={{ scale: 0.98 }}
            className="inline-flex h-10 w-full items-center justify-center rounded-xl bg-gradient-to-l from-[#8B5CF6] to-[#6D28D9] text-[12.5px] font-bold text-white shadow-[0_0_16px_rgba(139,92,246,0.4)]"
          >
            عضویت
          </motion.button>
        </form>
      )}
    </aside>
  );
}
