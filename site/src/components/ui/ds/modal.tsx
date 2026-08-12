"use client";

import { useEffect, useId } from "react";
import { Button } from "@/components/ui/ds/button";

type ModalProps = {
  open: boolean;
  onClose: () => void;
  title: string;
  children: React.ReactNode;
  footer?: React.ReactNode;
};

export function Modal({ open, onClose, title, children, footer }: ModalProps) {
  const titleId = useId();

  useEffect(() => {
    if (!open) return;
    function onKey(event: KeyboardEvent) {
      if (event.key === "Escape") onClose();
    }
    window.addEventListener("keydown", onKey);
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    return () => {
      window.removeEventListener("keydown", onKey);
      document.body.style.overflow = prev;
    };
  }, [open, onClose]);

  if (!open) return null;

  return (
    <div
      className="fixed inset-0 z-[100] flex items-center justify-center bg-black/70 p-4 backdrop-blur-sm"
      role="presentation"
      onMouseDown={(e) => {
        if (e.target === e.currentTarget) onClose();
      }}
    >
      <div
        role="dialog"
        aria-modal="true"
        aria-labelledby={titleId}
        dir="rtl"
        className="ds-slide w-full max-w-md rounded-[var(--ds-radius-xl)] border border-[color:var(--ds-border-strong)] bg-[color:var(--ds-bg-elevated)] p-5 shadow-[var(--ds-shadow-md)]"
      >
        <div className="mb-4 flex items-start justify-between gap-3">
          <h2 id={titleId} className="text-lg font-extrabold text-[color:var(--ds-fg)]">
            {title}
          </h2>
          <Button variant="ghost" size="sm" onClick={onClose} aria-label="بستن">
            بستن
          </Button>
        </div>
        <div className="text-sm leading-7 text-[color:var(--ds-muted)]">{children}</div>
        {footer ? <div className="mt-5 flex flex-wrap justify-end gap-2">{footer}</div> : null}
      </div>
    </div>
  );
}
