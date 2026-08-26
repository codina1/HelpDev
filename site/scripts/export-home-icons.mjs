import sharp from "sharp";
import fs from "fs";
import path from "path";

const ASSETS =
  "C:/Users/win10/.cursor/projects/e-project-HelpDev/assets";
const OUT = "E:/project/HelpDev/site/public/home";
const DEBUG = path.join(OUT, "_crop_debug");

const sheetSrc = path.join(
  ASSETS,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_7c6d0529-3ff6-4ae1-bc28-52bca6a8745d-97ccb226-95f4-4b1a-87bd-c6e849fe5617.png",
);
const bookSrc = path.join(
  ASSETS,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_6bc90387-5037-4a1d-ba32-302900456f3f-5b01e985-0c05-494e-b450-beee61156241.png",
);
const heroSrc = path.join(
  ASSETS,
  "c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_e5419264-a9b8-4d35-997c-06d4ce48f59f-573ab67a-b597-4cc2-a912-5bdaffac40f8.png",
);

/** Detect non-black blobs (same logic as import pass). */
async function detectBoxes(src) {
  const { data, info } = await sharp(src).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  const w = info.width;
  const h = info.height;
  const visited = new Uint8Array(w * h);
  const boxes = [];
  const thresh = 18;
  const bright = (x, y) => {
    const i = (y * w + x) * 4;
    return data[i] > thresh || data[i + 1] > thresh || data[i + 2] > thresh;
  };

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
        cy: (minY + maxY) / 2,
        cx: (minX + maxX) / 2,
      });
    }
  }
  boxes.sort((a, b) => a.cy - b.cy || a.cx - b.cx);
  return { boxes, w, h };
}

async function makeTransparentPng(input, outPath, size = 256) {
  const { data, info } = await sharp(input)
    .ensureAlpha()
    .raw()
    .toBuffer({ resolveWithObject: true });

  for (let i = 0; i < data.length; i += 4) {
    const r = data[i];
    const g = data[i + 1];
    const b = data[i + 2];
    // punch out near-black background
    if (r < 22 && g < 22 && b < 22) {
      data[i + 3] = 0;
    }
  }

  await sharp(data, { raw: { width: info.width, height: info.height, channels: 4 } })
    .resize(size, size, {
      fit: "contain",
      background: { r: 0, g: 0, b: 0, alpha: 0 },
    })
    .png()
    .toFile(outPath);
}

async function cropBox(src, box, pad = 8) {
  const meta = await sharp(src).metadata();
  const left = Math.max(0, box.minX - pad);
  const top = Math.max(0, box.minY - pad);
  const width = Math.min(meta.width - left, box.bw + pad * 2);
  const height = Math.min(meta.height - top, box.bh + pad * 2);
  return sharp(src).extract({ left, top, width, height }).png().toBuffer();
}

await sharp(heroSrc).png().toFile(path.join(OUT, "hero-workspace.png"));
await sharp(heroSrc).webp({ quality: 88 }).toFile(path.join(OUT, "hero-workspace.webp"));
console.log("hero ok");

{
  const meta = await sharp(bookSrc).metadata();
  const size = Math.min(meta.width, meta.height);
  const left = Math.floor((meta.width - size) / 2);
  const top = Math.floor((meta.height - size) / 2);
  const buf = await sharp(bookSrc)
    .extract({ left, top, width: size, height: size })
    .png()
    .toBuffer();
  await makeTransparentPng(buf, path.join(OUT, "icon-learning.png"), 320);
  console.log("learning ok");
}

const { boxes } = await detectBoxes(sheetSrc);
console.log("detected", boxes.length);

// Manual name map by detected index from previous run
const MAP = {
  0: ["icon-book-alt.png", 280],
  1: ["icon-roadmap.png", 280],
  2: ["icon-tools.png", 280],
  3: ["icon-prompt.png", 280],
  4: ["icon-news.png", 280],
  5: ["icon-code.png", 220],
  6: ["icon-architect.png", 220],
  7: ["icon-db.png", 220],
  8: ["icon-security.png", 220],
  9: ["icon-base64.png", 220],
  10: ["icon-jwt.png", 220],
  11: ["icon-markdown.png", 220],
  12: ["icon-scan.png", 220],
  13: ["icon-devops.png", 220],
  14: ["icon-mobile.png", 220],
  15: ["icon-dotnet.png", 220],
  16: ["icon-frontend.png", 220],
  17: ["icon-backend.png", 220],
  18: ["icon-database.png", 220],
  19: ["icon-security-alt.png", 220],
  20: ["icon-ai.png", 220],
  21: ["icon-linux.png", 220],
  22: ["icon-prompt-lab.png", 320],
};

for (const [indexStr, [name, size]] of Object.entries(MAP)) {
  const i = Number(indexStr);
  const box = boxes[i];
  if (!box) {
    console.warn("missing box", i);
    continue;
  }
  const buf = await cropBox(sheetSrc, box);
  await makeTransparentPng(buf, path.join(OUT, name), size);
  console.log("wrote", name);
}

// Prefer dedicated learning book already written; keep sheet book as alt.
fs.rmSync(DEBUG, { recursive: true, force: true });
console.log("debug cleaned");
