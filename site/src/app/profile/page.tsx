import { Suspense } from "react";
import { AccountPageContent } from "@/components/account/account-page-content";

export default function ProfilePage() {
  return (
    <Suspense
      fallback={
        <div className="ui-panel p-6 text-[13px] text-slate-400">
          در حال بارگذاری حساب کاربری...
        </div>
      }
    >
      <AccountPageContent />
    </Suspense>
  );
}
