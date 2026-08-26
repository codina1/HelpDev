import sharp from 'sharp';
import fs from 'fs';
import path from 'path';

const base =
  'C:/Users/win10/.cursor/projects/e-project-HelpDev/assets';
const outDir = 'public/home';
fs.mkdirSync(path.join(outDir, 'paths'), { recursive: true });

/** One unique icon per path — no semantic duplicates. */
const selected = {
  ai: 'c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__05_54_28_PM-0ce342d9-8877-417d-bf62-75d2a7d8b248.jpg',
  backend:
    'c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__05_52_32_PM-9f0947a4-ac39-4447-91db-09531010dfe5.png',
  dotnet:
    'c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__05_54_34_PM-26a65cb7-4811-4347-a4dc-dbc71a2caf10.jpg',
  devops:
    'c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__05_52_24_PM-d936ceb6-6d1f-466a-a7d6-e55c2c3f7423.png',
  frontend:
    'c__Users_win10_AppData_Roaming_Cursor_User_workspaceStorage_6bbe3234da46d5f47119feda59c918a6_images_ChatGPT_Image_Aug_26__2026__05_54_15_PM-e78021d9-d0a3-4050-b99d-66bf6c2f8604.jpg',
};

function removeMatte(data, w, h) {
  const N = w * h;
  const mark = new Uint8Array(N);
  const idx = (x, y) => y * w + x;

  const isWhite = (i) => {
    const p = i * 4;
    const r = data[p];
    const g = data[p + 1];
    const b = data[p + 2];
    const min = Math.min(r, g, b);
    const lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;
    return min >= 235 || lum >= 245;
  };

  const isDarkTile = (i) => {
    const p = i * 4;
    const r = data[p];
    const g = data[p + 1];
    const b = data[p + 2];
    const max = Math.max(r, g, b);
    const min = Math.min(r, g, b);
    const lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;
    // Near-black / navy matte (not saturated purple/blue icon glow)
    return max <= 72 && lum <= 48 && max - min <= 42;
  };

  const flood = (predicate) => {
    const q = [];
    const push = (x, y) => {
      if (x < 0 || y < 0 || x >= w || y >= h) return;
      const i = idx(x, y);
      if (mark[i] || !predicate(i)) return;
      mark[i] = 1;
      q.push(i);
    };
    for (let x = 0; x < w; x++) {
      push(x, 0);
      push(x, h - 1);
    }
    for (let y = 0; y < h; y++) {
      push(0, y);
      push(w - 1, y);
    }
    // Also seed from already-transparent / marked neighbors for second pass
    for (let y = 0; y < h; y++) {
      for (let x = 0; x < w; x++) {
        const i = idx(x, y);
        if (!mark[i]) continue;
        push(x - 1, y);
        push(x + 1, y);
        push(x, y - 1);
        push(x, y + 1);
      }
    }
    while (q.length) {
      const i = q.pop();
      const x = i % w;
      const y = (i - x) / w;
      push(x - 1, y);
      push(x + 1, y);
      push(x, y - 1);
      push(x, y + 1);
    }
  };

  flood(isWhite);
  flood(isDarkTile);

  // Punch enclosed dark fills (neon icon interiors) — keep saturated glow/faces
  for (let i = 0; i < N; i++) {
    if (mark[i]) continue;
    if (isDarkTile(i)) mark[i] = 1;
  }

  // Dilate transparency 2px to clean anti-aliased matte edges
  let current = mark;
  for (let pass = 0; pass < 2; pass++) {
    const next = new Uint8Array(current);
    for (let y = 1; y < h - 1; y++) {
      for (let x = 1; x < w - 1; x++) {
        const i = idx(x, y);
        if (current[i]) continue;
        if (
          current[i - 1] ||
          current[i + 1] ||
          current[i - w] ||
          current[i + w]
        ) {
          // Only eat near-matte fringe, not vivid icon pixels
          if (isWhite(i) || isDarkTile(i) || isFringe(data, i)) next[i] = 1;
        }
      }
    }
    current = next;
  }

  for (let i = 0; i < N; i++) if (current[i]) data[i * 4 + 3] = 0;
}

function isFringe(data, i) {
  const p = i * 4;
  const r = data[p];
  const g = data[p + 1];
  const b = data[p + 2];
  const max = Math.max(r, g, b);
  const min = Math.min(r, g, b);
  const lum = 0.2126 * r + 0.7152 * g + 0.0722 * b;
  return (max <= 70 && max - min <= 35) || lum >= 230;
}

async function processIcon(src, dest) {
  const { data, info } = await sharp(src)
    .ensureAlpha()
    .resize(320, 320, {
      fit: 'contain',
      background: { r: 255, g: 255, b: 255, alpha: 0 },
    })
    .raw()
    .toBuffer({ resolveWithObject: true });

  removeMatte(data, info.width, info.height);

  // Trim transparent padding
  const buf = await sharp(data, {
    raw: { width: info.width, height: info.height, channels: 4 },
  })
    .trim({ threshold: 0 })
    .resize(256, 256, {
      fit: 'contain',
      background: { r: 0, g: 0, b: 0, alpha: 0 },
    })
    .png()
    .toBuffer();

  fs.writeFileSync(dest, buf);
  const meta = await sharp(buf).ensureAlpha().raw().toBuffer({ resolveWithObject: true });
  let trans = 0;
  for (let i = 3; i < meta.data.length; i += 4) if (meta.data[i] === 0) trans++;
  console.log(
    'ok',
    path.basename(dest),
    buf.length,
    'trans%',
    ((100 * trans) / (meta.info.width * meta.info.height)).toFixed(1),
  );
}

for (const [id, file] of Object.entries(selected)) {
  const src = path.join(base, file);
  if (!fs.existsSync(src)) {
    console.error('MISSING', id, src);
    process.exit(1);
  }
  await processIcon(src, path.join(outDir, `icon-${id}.png`));
  await processIcon(src, path.join(outDir, 'paths', `path-${id}.png`));
}
console.log('done');
