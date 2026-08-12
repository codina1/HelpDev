"use client";

import { useEffect } from "react";
import { AdminErrorState } from "@/components/admin/feedback/admin-error-state";

export default function AdminError({
  error,
  reset,
}: {
  error: Error & { digest?: string };
  reset: () => void;
}) {
  useEffect(() => {
    // Log to the console only; never surface raw errors in the UI.
    console.error(error);
  }, [error]);

  return (
    <div className="py-8">
      <AdminErrorState
        title="خطای غیرمنتظره"
        error={error}
        onRetry={reset}
      />
    </div>
  );
}
