"use client";

import { useState } from "react";
import { getContentWorkspace } from "@/lib/admin/content/registry";
import {
  FoundationWorkspaceShell,
  FutureSaveBar,
} from "@/components/admin/content/workspaces/foundation-workspace-shell";
import { AdminSurface } from "@/components/admin/page/admin-surface";
import { FutureCapabilityList } from "@/components/admin/content/workspaces/future-capability-list";

const workspace = getContentWorkspace("comparison");

export function ComparisonList() {
  return <FoundationWorkspaceShell workspace={workspace} mode="list" />;
}

export function ComparisonEditor() {
  const [title, setTitle] = useState("");
  const [left, setLeft] = useState("");
  const [right, setRight] = useState("");

  return (
    <FoundationWorkspaceShell workspace={workspace} mode="create">
      <div className="grid grid-cols-1 gap-4 lg:grid-cols-2">
        <AdminSurface className="space-y-4">
          <h2 className="adm-text text-[14px] font-bold">مقایسه</h2>
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">عنوان</span>
            <input className="adm-input" value={title} onChange={(e) => setTitle(e.target.value)} />
          </label>
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">آیتم چپ</span>
            <input className="adm-input" value={left} onChange={(e) => setLeft(e.target.value)} />
          </label>
          <label className="block space-y-1.5">
            <span className="adm-text text-[12px] font-semibold">آیتم راست</span>
            <input className="adm-input" value={right} onChange={(e) => setRight(e.target.value)} />
          </label>
        </AdminSurface>
        <div className="space-y-4">
          <FutureCapabilityList
            items={["جدول معیارها", "امتیازدهی", "نمودار مقایسه", "پیوند به ابزار/مقاله"]}
          />
          <FutureSaveBar label="ذخیره مقایسه" />
        </div>
      </div>
    </FoundationWorkspaceShell>
  );
}
