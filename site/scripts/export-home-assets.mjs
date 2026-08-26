import sharp from "sharp";
import fs from "fs";
import path from "path";
import { fileURLToPath } from "url";

const __dirname = path.dirname(fileURLToPath(import.meta.url));
const out = path.join(__dirname, "../public/home");
const assets =
  "C:/Users/win10/.cursor/projects/e-project-HelpDev/assets";

const heroSrc = path.join(
  assets,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__09_49_18_AM-f0d47ad5-414d-40ca-aef5-19cc703e5d99.jpg",
);
const bookSrc = path.join(
  assets,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__09_01_33_AM-48e17703-6f26-46b5-80eb-912afc0460aa.jpg",
);
const sheetSrc = path.join(
  assets,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__09_20_48_AM-4c06265a-749e-413e-9b48-41041bb0e3a7.jpg",
);

async function knockBlack(input, output, threshold = 24) {
  const { data, info } = await sharp(input).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  for (let i = 0; i < data.length; i += 4) {
    if (data[i] <= threshold && data[i + 1] <= threshold && data[i + 2] <= threshold) {
      data[i + 3] = 0;
    }
  }
  await sharp(data, { raw: { width: info.width, height: info.height, channels: 4 } })
    .png()
    .toFile(output);
  return info;
}

/** Content bbox ignoring near-black / transparent pixels. */
async function contentBox(file, threshold = 30) {
  const { data, info } = await sharp(file).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  let minX = info.width;
  let minY = info.height;
  let maxX = 0;
  let maxY = 0;
  for (let y = 0; y < info.height; y++) {
    for (let x = 0; x < info.width; x++) {
      const i = (y * info.width + x) * 4;
      const a = data[i + 3];
      const lum = (data[i] + data[i + 1] + data[i + 2]) / 3;
      if (a > 20 && lum > threshold) {
        if (x < minX) minX = x;
        if (y < minY) minY = y;
        if (x > maxX) maxX = x;
        if (y > maxY) maxY = y;
      }
    }
  }
  if (maxX < minX) return null;
  const pad = 8;
  return {
    left: Math.max(0, minX - pad),
    top: Math.max(0, minY - pad),
    width: Math.min(info.width - Math.max(0, minX - pad), maxX - minX + pad * 2),
    height: Math.min(info.height - Math.max(0, minY - pad), maxY - minY + pad * 2),
  };
}

async function finalizeIcon(input, output, size = 256) {
  const box = await contentBox(input);
  let pipeline = sharp(input);
  if (box) pipeline = pipeline.extract(box);
  await pipeline
    .resize({ width: size, height: size, fit: "contain", background: { r: 0, g: 0, b: 0, alpha: 0 } })
    .png()
    .toFile(output);
}

fs.mkdirSync(out, { recursive: true });

// Clean previous generated icons (keep SVGs)
for (const f of fs.readdirSync(out)) {
  if (/^(icon-|hero-workspace)/.test(f)) fs.unlinkSync(path.join(out, f));
}

await sharp(heroSrc)
  .resize({ width: 1200, withoutEnlargement: true })
  .webp({ quality: 88 })
  .toFile(path.join(out, "hero-workspace.webp"));
await sharp(heroSrc)
  .resize({ width: 1200, withoutEnlargement: true })
  .png({ compressionLevel: 9 })
  .toFile(path.join(out, "hero-workspace.png"));
console.log("hero ok");

const bookTmp = path.join(out, "_book-tmp.png");
await knockBlack(bookSrc, bookTmp, 20);
await finalizeIcon(bookTmp, path.join(out, "icon-learning.png"), 320);
fs.unlinkSync(bookTmp);
console.log("book ok");

const sheetMeta = await sharp(sheetSrc).metadata();
const W = sheetMeta.width;
const H = sheetMeta.height;
const sheetKnock = path.join(out, "_sheet-tmp.png");
await knockBlack(sheetSrc, sheetKnock, 16);

// Top row only — short strip so mid-row tiles are excluded
const topStripH = Math.round(H * 0.30);
const cellW = Math.floor(W / 5);
const topIcons = [
  { name: "learning-sheet", i: 0 },
  { name: "roadmap", i: 1 },
  { name: "prompt", i: 2 },
  { name: "tools", i: 3 },
  { name: "news", i: 4 },
];

for (const { name, i } of topIcons) {
  const left = i * cellW;
  const width = i === 4 ? W - left : cellW;
  const tmp = path.join(out, `_cell-${name}.png`);
  await sharp(sheetKnock).extract({ left, top: 0, width, height: topStripH }).png().toFile(tmp);
  await finalizeIcon(tmp, path.join(out, `icon-${name}.png`), 256);
  fs.unlinkSync(tmp);
  console.log("top", name);
}

// Mid row framed icons (8 cells) — for toolbox / categories
const midTop = Math.round(H * 0.30);
const midH = Math.round(H * 0.28);
const midCell = Math.floor(W / 8);
const midIcons = [
  { name: "code", i: 0 },
  { name: "jwt", i: 1 },
  { name: "architect", i: 2 },
  { name: "db", i: 3 },
  { name: "security", i: 4 },
  { name: "markdown", i: 5 },
  { name: "base64", i: 6 },
];
for (const { name, i } of midIcons) {
  const left = i * midCell;
  const width = i === 7 ? W - left : midCell;
  const tmp = path.join(out, `_mid-${name}.png`);
  await sharp(sheetKnock)
    .extract({ left, top: midTop, width, height: Math.min(midH, H - midTop) })
    .png()
    .toFile(tmp);
  await finalizeIcon(tmp, path.join(out, `icon-${name}.png`), 192);
  fs.unlinkSync(tmp);
  console.log("mid", name);
}

// Bottom tech row (9 cells)
const row3Top = Math.round(H * 0.56);
const row3H = Math.round(H * 0.24);
const row3Cell = Math.floor(W / 9);
const row3Icons = [
  { name: "ai", i: 0 },
  { name: "dotnet", i: 1 },
  { name: "frontend", i: 2 },
  { name: "backend", i: 3 },
  { name: "devops", i: 4 },
  { name: "mobile", i: 5 },
  { name: "database", i: 6 },
  { name: "firewall", i: 7 },
  { name: "linux", i: 8 },
];
for (const { name, i } of row3Icons) {
  const left = i * row3Cell;
  const width = i === 8 ? W - left : row3Cell;
  const tmp = path.join(out, `_r3-${name}.png`);
  await sharp(sheetKnock)
    .extract({ left, top: row3Top, width, height: Math.min(row3H, H - row3Top) })
    .png()
    .toFile(tmp);
  await finalizeIcon(tmp, path.join(out, `icon-${name}.png`), 192);
  fs.unlinkSync(tmp);
  console.log("row3", name);
}

fs.unlinkSync(sheetKnock);

const files = fs
  .readdirSync(out)
  .filter((f) => f.startsWith("icon-") || f.startsWith("hero-workspace"))
  .sort();
for (const f of files) {
  const st = fs.statSync(path.join(out, f));
  const meta = await sharp(path.join(out, f)).metadata();
  console.log(`${f} ${meta.width}x${meta.height} ${(st.size / 1024).toFixed(1)}KB`);
}
