/**
 * The app's public origin, used for canonical/Open Graph URL resolution (layout.tsx's
 * `metadataBase`) and for the absolute URLs in sitemap.ts and robots.ts.
 *
 * Why this exists rather than reading the env var at each call site: the fallback below was
 * previously duplicated in three files as
 *
 *     process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3030"
 *
 * which looks correct but silently breaks the production image build. `??` only falls back on
 * `null`/`undefined`, and apps/web/Dockerfile sets `ENV NEXT_PUBLIC_SITE_URL=$NEXT_PUBLIC_SITE_URL`
 * — so when the build arg isn't passed (as in CI's plain `docker build`), the variable is defined
 * but *empty*, the fallback never fires, and `new URL("")` throws ERR_INVALID_URL during static
 * generation. The failure surfaced as "Failed to collect page data for /_not-found", which points
 * nowhere near the actual cause. Treating empty as absent, in one place, is what fixes it.
 *
 * A genuinely malformed value (e.g. a typo'd "htp://...") is deliberately NOT swallowed here: that
 * is a real misconfiguration and should fail the build loudly rather than silently serve canonical
 * URLs pointing at localhost. Only "not provided" falls back.
 *
 * Note `process.env.NEXT_PUBLIC_SITE_URL` must appear as a static expression for Next.js to inline
 * it at build time — don't refactor this into a dynamic property lookup.
 */
const configured = process.env.NEXT_PUBLIC_SITE_URL?.trim();

/** Dev default matches `npm run dev`'s port (see .claude/launch.json). */
const FALLBACK_SITE_URL = "http://localhost:3030";

/**
 * Absolute origin with any trailing slash removed, so callers can safely append a path
 * (`${SITE_URL}/sitemap.xml`) without producing a double slash.
 */
export const SITE_URL = (configured || FALLBACK_SITE_URL).replace(/\/+$/, "");
