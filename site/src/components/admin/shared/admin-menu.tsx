"use client";

import {
  useCallback,
  useEffect,
  useId,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { usePathname } from "next/navigation";

type AdminMenuAlign = "start" | "end";

type AdminMenuProps = {
  /** Renders the trigger. `open` reflects the current state for aria wiring. */
  trigger: (args: {
    open: boolean;
    toggle: () => void;
    triggerProps: {
      "aria-haspopup": "menu";
      "aria-expanded": boolean;
      "aria-controls": string;
    };
  }) => ReactNode;
  children: (args: { close: () => void }) => ReactNode;
  align?: AdminMenuAlign;
  panelClassName?: string;
  label?: string;
};

/**
 * Headless dropdown used by header widgets. Handles outside-click, Escape to
 * close, and auto-closes on route change. RTL-aware alignment.
 */
export function AdminMenu({
  trigger,
  children,
  align = "end",
  panelClassName = "",
  label,
}: AdminMenuProps) {
  const [open, setOpen] = useState(false);
  const containerRef = useRef<HTMLDivElement>(null);
  const panelId = useId();
  const pathname = usePathname();

  const close = useCallback(() => setOpen(false), []);
  const toggle = useCallback(() => setOpen((value) => !value), []);

  useEffect(() => {
    setOpen(false);
  }, [pathname]);

  useEffect(() => {
    if (!open) return;

    const onPointerDown = (event: MouseEvent) => {
      if (!containerRef.current?.contains(event.target as Node)) {
        setOpen(false);
      }
    };
    const onKeyDown = (event: KeyboardEvent) => {
      if (event.key === "Escape") setOpen(false);
    };

    document.addEventListener("mousedown", onPointerDown);
    document.addEventListener("keydown", onKeyDown);
    return () => {
      document.removeEventListener("mousedown", onPointerDown);
      document.removeEventListener("keydown", onKeyDown);
    };
  }, [open]);

  return (
    <div ref={containerRef} className="relative">
      {trigger({
        open,
        toggle,
        triggerProps: {
          "aria-haspopup": "menu",
          "aria-expanded": open,
          "aria-controls": panelId,
        },
      })}
      {open ? (
        <div
          id={panelId}
          role="menu"
          aria-label={label}
          className={`adm-panel adm-animate-in absolute top-[calc(100%+8px)] z-40 min-w-[220px] p-1.5 ${
            align === "end" ? "end-0" : "start-0"
          } ${panelClassName}`.trim()}
        >
          {children({ close })}
        </div>
      ) : null}
    </div>
  );
}
