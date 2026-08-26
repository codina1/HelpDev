import sharp from "sharp";
import fs from "fs";
import path from "path";

const ASSETS =
  "C:/Users/win10/.cursor/projects/e-project-HelpDev/assets";
const OUT = "E:/project/HelpDev/site/public/home";

const heroSrc = path.join(
  ASSETS,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_e5419264-a9b8-4d35-997c-06d4ce48f59f-573ab67a-b597-4cc2-a912-5bdaffac40f8.png",
);
const bookSrc = path.join(
  ASSETS,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_6bc90387-5037-4a1d-ba32-302900456f3f-5b01e985-0c05-494e-b450-beee61156241.png",
);
const sheetSrc = path.join(
  ASSETS,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_7c6d0529-3ff6-4ae1-bc28-52bca6a8745d-97ccb226-95f4-4b1a-87bd-c6e849fe5617.png",
);

await sharp(heroSrc).png().toFile(path.join(OUT, "hero-workspace.png"));
await sharp(heroSrc).webp({ quality: 86 }).toFile(path.join(OUT, "hero-workspace.webp"));
console.log("hero updated");

{
  const meta = await sharp(bookSrc).metadata();
  const size = Math.min(meta.width, meta.height);
  const left = Math.floor((meta.width - size) / 2);
  const top = Math.floor((meta.height - size) / 2);
  await sharp(bookSrc)
    .extract({ left, top, width: size, height: size })
    .resize(320, 320)
    .png()
    .toFile(path.join(OUT, "icon-learning.png"));
  console.log("learning book updated", size);
}

const { data, info } = await sharp(sheetSrc).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
const w = info.width;
const h = info.height;
const visited = new Uint8Array(w * h);
const boxes = [];
const thresh = 18;

function bright(x, y) {
  const i = (y * w + x) * 4;
  return data[i] > thresh || data[i + 1] > thresh || data[i + 2] > thresh;
}

for (let y = 0; y < h; y++) {
  for (let x = 0; x < w; x++) {
    const p = y * w + x;
    if (visited[p] || !bright(x, y)) continue;
    let minX = x;
    let maxX = x;
    let minY = y;
    let maxY = y;
    let count = 0;
    const q = [p];
    visited[p] = 1;
    while (q.length) {
      const cur = q.pop();
      const cx = cur % w;
      const cy = (cur - cx) / w;
      count++;
      if (cx < minX) minX = cx;
      if (cx > maxX) maxX = cx;
      if (cy < minY) minY = cy;
      if (cy > maxY) maxY = cy;
      for (const [nx, ny] of [
        [cx - 1, cy],
        [cx + 1, cy],
        [cx, cy - 1],
        [cx, cy + 1],
      ]) {
        if (nx < 0 || ny < 0 || nx >= w || ny >= h) continue;
        const np = ny * w + nx;
        if (visited[np] || !bright(nx, ny)) continue;
        visited[np] = 1;
        q.push(np);
      }
    }
    const bw = maxX - minX + 1;
    const bh = maxY - minY + 1;
    if (count < 800 || bw < 28 || bh < 28) continue;
    boxes.push({
      minX,
      minY,
      maxX,
      maxY,
      bw,
      bh,
      count,
      area: bw * bh,
      cy: (minY + maxY) / 2,
      cx: (minX + maxX) / 2,
    });
  }
}

boxes.sort((a, b) => a.cy - b.cy || a.cx - b.cx);
console.log("boxes", boxes.length);
for (const [i, b] of boxes.entries()) {
  console.log(
    i,
    `x=${b.minX}-${b.maxX} y=${b.minY}-${b.maxY} ${b.bw}x${b.bh} area=${b.area}`,
  );
}

const debugDir = path.join(OUT, "_crop_debug");
fs.mkdirSync(debugDir, { recursive: true });
for (const [i, b] of boxes.entries()) {
  const pad = 6;
  const left = Math.max(0, b.minX - pad);
  const top = Math.max(0, b.minY - pad);
  const width = Math.min(w - left, b.bw + pad * 2);
  const height = Math.min(h - top, b.bh + pad * 2);
  await sharp(sheetSrc)
    .extract({ left, top, width, height })
    .png()
    .toFile(path.join(debugDir, `${String(i).padStart(2, "0")}.png`));
}
console.log("debug crops written to", debugDir);
