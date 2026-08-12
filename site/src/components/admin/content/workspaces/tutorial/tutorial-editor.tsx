"use client";

import { useState } from "react";
import { getContentWorkspace } from "@/lib/admin/content/registry";
import {
  FoundationWorkspaceShell,
  FutureSaveBar,
} from "@/components/admin/content/workspaces/foundation-workspace-shell";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { FutureCapabilityList } from "@/components/admin/content/workspaces/future-capability-list";

const workspace = getContentWorkspace("tutorial");

export function TutorialList() {
  return <FoundationWorkspaceShell workspace={workspace} mode="list" />;
}

export function TutorialEditor() {
  const [title, setTitle] = useState("");
  const [summary, setSummary] = useState("");
  const [body, setBody] = useState("");

  return (
    <FoundationWorkspaceShell workspace={workspace} mode="create">
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <AdminSurface className="space-y-4">
          <h2 className="adm-text text-[14px] font-bold">آموزش کوتاه</h2>
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">عنوان</span>
            <input className="adm-input" value={title} onChange={(e) => setTitle(e.target.value)} />
          </label>
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">خلاصه</span>
            <input className="adm-input" value={summary} onChange={(e) => setSummary(e.target.value)} />
          </label>
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">متن</span>
            <textarea
              className="adm-input min-h-[160px]"
              value={body}
              onChange={(e) => setBody(e.target.value)}
            />
          </label>
        </AdminSurface>
        <div className="space-y-4">
          <FutureCapabilityList
            items={["مراحل شماره‌دار", "ویدیو/مدیا", "چک‌لیست پیشرفت", "پیوند به دوره"]}
          />
          <FutureSaveBar label="ذخیره آموزش" />
        </div>
      </div>
    </FoundationWorkspaceShell>
  );
}
