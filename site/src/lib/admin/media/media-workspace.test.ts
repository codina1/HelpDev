import { describe, expect, it } from "vitest";
import { readdirSync, readFileSync, statSync } from "node:fs";
import { join } from "node:path";

const WORKSPACE_FILE = join(
  process.cwd(),
  "src/components/admin/media/media-workspace.tsx",
);
const PICKER_FILE = join(
  process.cwd(),
  "src/components/admin/media/media-picker-dialog.tsx",
);
const UPLOAD_DIALOG_FILE = join(
  process.cwd(),
  "src/components/admin/media/media-upload-dialog.tsx",
);
const DROPZONE_FILE = join(
  process.cwd(),
  "src/components/admin/media/media-dropzone.tsx",
);
const DETAIL_PANEL_FILE = join(
  process.cwd(),
  "src/components/admin/media/media-detail-panel.tsx",
);
const NAVIGATION_FILE = join(process.cwd(), "src/lib/admin/navigation.ts");
const ROUTES_FILE = join(process.cwd(), "src/lib/admin/routes.ts");
const MEDIA_DIRS = [
  join(process.cwd(), "src/lib/admin/media"),
  join(process.cwd(), "src/components/admin/media"),
];

function collect(dir: string, acc: string[] = []): string[] {
  for (const entry of readdirSync(dir)) {
    const full = join(dir, entry);
    if (statSync(full).isDirectory()) {
      collect(full, acc);
      continue;
    }
    if (!/\.(ts|tsx)$/.test(entry)) continue;
    if (/\.test\.(ts|tsx)$/.test(entry)) continue;
    acc.push(full);
  }
  return acc;
}

describe("Media navigation + routes are wired up", () => {
  it("registers /admin/media in ADMIN_ROUTES", () => {
    const routes = readFileSync(ROUTES_FILE, "utf8");
    expect(routes).toMatch(/media:\s*["']\/admin\/media["']/);
  });

  it("enables the content-media nav item (status ready, with an href)", () => {
    const navigation = readFileSync(NAVIGATION_FILE, "utf8");
    const contentMediaBlock = navigation.slice(navigation.indexOf('"content-media"'));
    const itemSource = contentMediaBlock.slice(0, contentMediaBlock.indexOf("},"));
    expect(itemSource).toContain('status: "ready"');
    expect(itemSource).toContain("href: ADMIN_ROUTES.media");
  });
});

describe("MediaWorkspace wiring", () => {
  const source = readFileSync(WORKSPACE_FILE, "utf8");

  it("uses the real data hooks (list + upload), not fabricated state", () => {
    expect(source).toContain("useAdminMediaList");
    expect(source).toContain("MediaUploadDialog");
  });

  it("is URL-driven for page/pageSize/search", () => {
    expect(source).toContain("parseAdminMediaListQuery");
    expect(source).toContain("mergeAdminMediaListQuery");
    expect(source).toContain("buildAdminMediaListHref");
  });
});

describe("MediaPickerDialog contract", () => {
  const source = readFileSync(PICKER_FILE, "utf8");

  it("returns a picker selection via toMediaPickerSelection (id/publicUrl/altText/width/height)", () => {
    expect(source).toContain("toMediaPickerSelection");
    expect(source).toContain("onSelect");
  });

  it("supports upload inline and selects the freshly uploaded asset", () => {
    expect(source).toContain("MediaUploadDialog");
    expect(source).toContain("handleUploaded");
  });
});

describe("Upload dialog never fabricates progress and only uploads on explicit submit", () => {
  const source = readFileSync(UPLOAD_DIALOG_FILE, "utf8");

  it("has no fake percentage / progress bar", () => {
    expect(source).not.toMatch(/progress\s*%/i);
    expect(source).not.toMatch(/\d+%/);
    expect(source).not.toContain("setInterval");
  });

  it("only calls upload.upload(...) inside the submit handler, not on every keystroke", () => {
    const onChangeLines = source
      .split("\n")
      .filter((line) => /onChange=/.test(line));
    for (const line of onChangeLines) {
      expect(line).not.toContain("upload.upload(");
    }
    expect(source).toContain("const handleSubmit = useCallback(async () => {");
    expect(source).toContain("await upload.upload(");
  });
});

describe("Dropzone never accepts SVG and revokes object URLs", () => {
  const source = readFileSync(DROPZONE_FILE, "utf8");

  it("builds its accept attribute only from the JPEG/PNG/WebP allow-list", () => {
    expect(source).toContain("ACCEPTED_MEDIA_CONTENT_TYPES");
    expect(source).not.toMatch(/svg/i);
  });

  it("revokes the local preview object URL", () => {
    expect(source).toContain("URL.createObjectURL(file)");
    expect(source).toContain("URL.revokeObjectURL(url)");
  });
});

describe("Detail panel never exposes a storage key/filesystem path", () => {
  const source = readFileSync(DETAIL_PANEL_FILE, "utf8");

  it("only shows the public URL, filename, size, dimensions and dates", () => {
    expect(source).not.toMatch(/storageKey|filePath|diskPath/i);
    expect(source).toContain("absoluteUrl");
  });
});

describe("Media module guardrails", () => {
  it("has no delete endpoint, capability or UI action anywhere in the module", () => {
    const offenders: string[] = [];
    for (const dir of MEDIA_DIRS) {
      for (const file of collect(dir)) {
        const text = readFileSync(file, "utf8");
        if (/method:\s*["']DELETE["']/.test(text)) offenders.push(`${file} -> DELETE verb`);
        if (/deleteMediaAsset/i.test(text)) offenders.push(`${file} -> deleteMediaAsset`);
      }
    }
    expect(offenders, offenders.join("\n")).toHaveLength(0);
  });

  it("never persists file bytes or base64 into localStorage", () => {
    const offenders: string[] = [];
    for (const dir of MEDIA_DIRS) {
      for (const file of collect(dir)) {
        const text = readFileSync(file, "utf8");
        if (/localStorage/.test(text)) offenders.push(`${file} -> localStorage`);
        if (/base64/i.test(text)) offenders.push(`${file} -> base64`);
      }
    }
    expect(offenders, offenders.join("\n")).toHaveLength(0);
  });

  it("uses no unversioned /api/ literals", () => {
    const offenders: string[] = [];
    for (const dir of MEDIA_DIRS) {
      for (const file of collect(dir)) {
        if (/["'`]\/api\/(?!v1)/.test(readFileSync(file, "utf8"))) offenders.push(file);
      }
    }
    expect(offenders).toHaveLength(0);
  });
});
