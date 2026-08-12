# Media module

Dedicated **Media** module for the Admin CMS Media Library (Sprint 33). Images only; no video, documents, SVG, or cloud storage in v1.

## Boundaries

- `HelpDev.Modules.Media` — Domain, Application, Infrastructure in one module project (same pattern as Toolbox).
- **No cross-module references** from Content: `CoverImage` and `SeoMetadata.OgImage` remain URL strings; the picker writes `PublicUrl` only.
- `UploadedByUserId` is stored as an opaque `Guid` (no FK to `users` in the Media module).
- File bytes live in **object storage** (`IMediaStorage`); PostgreSQL stores metadata in `media_assets` only.

## Asset lifecycle (v1)

- Upload → inspect → store → persist → **Active**
- **No delete**, archive, or recycle bin in v1.
- **No Outbox events** (no subscribers).
- **No malware scanning** — signature/type/size/dimension checks only.

## Supported types

Allowlist (signature + decode):

- `image/jpeg`
- `image/png`
- `image/webp`

Rejected: SVG, HTML, PDF, executables, mismatched extension/signature.

Limits (config `Media` section, defaults in `appsettings.json`):

- `MaxUploadBytes` (default 5 MiB)
- `MaxWidth` / `MaxHeight` (default 8192)
- Optional `AltText` / `Caption` length bounds

## Storage

- **Abstraction:** `IMediaStorage` (Application)
- **V1 implementation:** `LocalMediaStorage` — root from `Media:LocalStorageRoot` (default `%LocalApplicationData%/HelpDev/media-uploads`, outside source tree)
- Keys: `yyyy/MM/{assetId}{.ext}` — server-generated; never the user filename
- Traversal-safe resolution; atomic temp write + rename; rollback `DeleteAsync` on DB failure only

Future: S3-compatible provider implementing `IMediaStorage`.

## Image inspection

- **SixLabors.ImageSharp** 3.1.11 (Infrastructure `ImageFileInspector`)
- Magic-byte checks + `Image.IdentifyAsync`; no HTML execution, no network

## Consistency strategy

1. Validate + inspect (in-memory buffer)
2. Store blob
3. `SaveChanges` once
4. On DB failure → best-effort storage cleanup (internal `DeleteAsync`)

## API (Admin)

Policy: `WriterOrAdmin`

| Method | Route | Notes |
| --- | --- | --- |
| POST | `/api/v1/admin/media` | `multipart/form-data`: `file`, optional `altText`, `caption` → **201** |
| GET | `/api/v1/admin/media` | Paged list; Writer scoped to own uploads |
| GET | `/api/v1/admin/media/{id}` | Detail; cross-owner Writer → **404** `media_asset_not_found` |

DTOs do **not** expose `storage_key` or filesystem paths.

## Public serving

- `GET /media/{year}/{month}/{fileName}` (`PublicMediaController`)
- Only keys under configured storage root; `Cache-Control: public, max-age=31536000, immutable`
- `X-Content-Type-Options: nosniff`

## Errors

| Code | Typical status |
| --- | --- |
| `media_asset_not_found` | 404 |
| `media_validation_failed` | 400 |
| `media_unsupported_type` | 415 |
| `media_payload_too_large` | 413 |
| `media_storage_failed` | 500 |

## Database

Table: `media_assets` (migration `AddMediaModuleV1`).

## Future

- Optional `MediaAssetId` on Content + usage tracking + orphan cleanup
- Cloud storage backend
- Metadata update endpoint
- Non-destructive “soft hide” before real deletion
