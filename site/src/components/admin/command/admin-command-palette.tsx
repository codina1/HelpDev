"use client";

import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useMemo,
  useRef,
  useState,
  type ReactNode,
} from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/components/auth";
import {
  ADMIN_NAVIGATION,
  filterAdminNavigation,
} from "@/lib/admin/navigation";
import { getPermissionsForRole } from "@/lib/admin/permissions";
import {
  buildCommandRegistry,
  searchCommands,
  type AdminCommand,
} from "@/lib/admin/command-menu";
import { AdminIcon } from "@/components/admin/shared/admin-icons";

type AdminCommandContextValue = {
  open: () => void;
  close: () => void;
  isOpen: boolean;
};

const AdminCommandContext = createContext<AdminCommandContextValue | null>(null);

export function AdminCommandProvider({ children }: { children: ReactNode }) {
  const [isOpen, setIsOpen] = useState(false);

  const open = useCallback(() => setIsOpen(true), []);
  const close = useCallback(() => setIsOpen(false), []);

  useEffect(() => {
    const onKeyDown = (event: KeyboardEvent) => {
      if ((event.metaKey || event.ctrlKey) && event.key.toLowerCase() === "k") {
        event.preventDefault();
        setIsOpen((value) => !value);
      }
    };
    window.addEventListener("keydown", onKeyDown);
    return () => window.removeEventListener("keydown", onKeyDown);
  }, []);

  const value = useMemo(() => ({ open, close, isOpen }), [open, close, isOpen]);

  return (
    <AdminCommandContext.Provider value={value}>
      {children}
      {isOpen ? <AdminCommandPalette onClose={close} /> : null}
    </AdminCommandContext.Provider>
  );
}

export function useAdminCommand(): AdminCommandContextValue {
  const context = useContext(AdminCommandContext);
  if (!context) {
    throw new Error("useAdminCommand must be used within an AdminCommandProvider");
  }
  return context;
}

function AdminCommandPalette({ onClose }: { onClose: () => void }) {
  const router = useRouter();
  const { user } = useAuth();
  const [query, setQuery] = useState("");
  const [activeIndex, setActiveIndex] = useState(0);
  const inputRef = useRef<HTMLInputElement>(null);

  const commands = useMemo(() => {
    const navigation = filterAdminNavigation(ADMIN_NAVIGATION, user?.role);
    return buildCommandRegistry(navigation, getPermissionsForRole(user?.role));
  }, [user?.role]);

  const results = useMemo(
    () => searchCommands(commands, query),
    [commands, query],
  );

  useEffect(() => {
    setActiveIndex(0);
  }, [query]);

  useEffect(() => {
    inputRef.current?.focus();
  }, []);

  const runCommand = useCallback(
    (command: AdminCommand | undefined) => {
      if (!command) return;
      onClose();
      router.push(command.href);
    },
    [onClose, router],
  );

  const onKeyDown = (event: React.KeyboardEvent) => {
    if (event.key === "Escape") {
      event.preventDefault();
      onClose();
      return;
    }
    if (event.key === "ArrowDown") {
      event.preventDefault();
      setActiveIndex((index) => Math.min(index + 1, results.length - 1));
      return;
    }
    if (event.key === "ArrowUp") {
      event.preventDefault();
      setActiveIndex((index) => Math.max(index - 1, 0));
      return;
    }
    if (event.key === "Enter") {
      event.preventDefault();
      runCommand(results[activeIndex]);
    }
  };

  return (
    <div
      className="fixed inset-0 z-[60] flex items-start justify-center p-4 pt-[12vh]"
      role="dialog"
      aria-modal="true"
      aria-label="جستجوی فرمان"
    >
      <button
        type="button"
        aria-label="بستن"
        onClick={onClose}
        className="absolute inset-0 bg-black/50"
      />
      <div
        className="adm-panel adm-animate-in relative z-10 w-full max-w-lg overflow-hidden p-0"
        onKeyDown={onKeyDown}
      >
        <div className="adm-border-b flex items-center gap-2 px-3 py-2.5">
          <AdminIcon name="search" size={18} className="adm-subtle shrink-0" />
          <input
            ref={inputRef}
            value={query}
            onChange={(event) => setQuery(event.target.value)}
            placeholder="جستجو یا اجرای فرمان..."
            className="w-full bg-transparent text-[14px] text-[var(--adm-text)] outline-none placeholder:text-[var(--adm-text-subtle)]"
            role="combobox"
            aria-expanded="true"
            aria-controls="admin-command-list"
            aria-autocomplete="list"
          />
          <kbd className="adm-subtle hidden rounded border border-[var(--adm-border)] px-1.5 py-0.5 text-[10px] sm:inline">
            ESC
          </kbd>
        </div>

        <ul
          id="admin-command-list"
          role="listbox"
          className="adm-scroll max-h-[50vh] overflow-y-auto p-1.5"
        >
          {results.length === 0 ? (
            <li className="adm-subtle px-3 py-6 text-center text-[13px]">
              نتیجه‌ای پیدا نشد
            </li>
          ) : (
            results.map((command, index) => (
              <li key={command.id} role="option" aria-selected={index === activeIndex}>
                <button
                  type="button"
                  onClick={() => runCommand(command)}
                  onMouseEnter={() => setActiveIndex(index)}
                  className={`adm-focus flex w-full items-center gap-3 rounded-lg px-3 py-2 text-start ${
                    index === activeIndex
                      ? "bg-[var(--adm-accent-soft)] text-[var(--adm-accent-text)]"
                      : "adm-muted"
                  }`}
                >
                  <AdminIcon name={command.icon} size={16} className="shrink-0" />
                  <span className="flex-1 truncate text-[13px] font-medium">
                    {command.title}
                  </span>
                  {command.subtitle ? (
                    <span className="adm-subtle text-[11px]">{command.subtitle}</span>
                  ) : null}
                </button>
              </li>
            ))
          )}
        </ul>
      </div>
    </div>
  );
}
