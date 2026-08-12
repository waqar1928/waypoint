import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Cookie Policy — Waypoint",
  description: "The complete list of cookies Waypoint sets and why.",
};

export default function CookiePolicyPage() {
  return (
    <article>
      <h1 className="font-display text-3xl font-semibold text-ink-900">Cookie Policy</h1>
      <p className="mt-2 text-sm text-ink-500">Last updated: 2026-08-11</p>

      <p className="mt-6 text-sm text-ink-700">
        This is the complete, actual list of cookies Waypoint sets today — every cookie this
        product uses anywhere, not a generic template list. There are exactly two, and both exist
        purely to make the product work; neither is used for advertising or tracking.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Cookies we set</h2>
      <div className="mt-4 overflow-x-auto">
        <table className="w-full border-collapse text-left text-sm">
          <thead>
            <tr className="border-b border-ink-300 text-ink-500">
              <th className="py-2 pr-4 font-medium">Name</th>
              <th className="py-2 pr-4 font-medium">Purpose</th>
              <th className="py-2 font-medium">Details</th>
            </tr>
          </thead>
          <tbody className="text-ink-700">
            <tr className="border-b border-ink-100">
              <td className="py-3 pr-4 font-mono text-xs">waypoint.auth</td>
              <td className="py-3 pr-4">Keeps you signed in.</td>
              <td className="py-3">
                Strictly necessary. HttpOnly (JavaScript can&apos;t read it), SameSite=Strict.
                Expires after 14 days of inactivity.
              </td>
            </tr>
            <tr>
              <td className="py-3 pr-4 font-mono text-xs">waypoint.csrf</td>
              <td className="py-3 pr-4">
                Protects your account from cross-site request forgery attacks.
              </td>
              <td className="py-3">
                Strictly necessary. Readable by the page&apos;s own JavaScript (required so the
                app can echo it back as a security header), but never sent anywhere except back to
                Waypoint&apos;s own servers.
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">What we don&apos;t use</h2>
      <p className="mt-3 text-sm text-ink-700">
        No advertising cookies, no third-party tracking pixels, and no analytics cookies — this
        product doesn&apos;t currently run any product analytics tool at all. If that changes in
        the future, this page (and this product&apos;s cookie-consent behavior) will be updated to
        reflect it honestly, rather than silently starting to track visitors under an unchanged
        policy.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Why there&apos;s no cookie banner</h2>
      <p className="mt-3 text-sm text-ink-700">
        Both cookies above are strictly necessary for the product to function (you can&apos;t stay
        signed in, or safely submit forms, without them) — under most cookie-consent frameworks,
        strictly-necessary cookies don&apos;t require a consent banner. This isn&apos;t a legal
        compliance certification; if you operate this product somewhere with stricter
        requirements, have that reviewed by counsel.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Questions</h2>
      <p className="mt-3 text-sm text-ink-700">
        See our <Link href="/contact" className="text-beacon-600 hover:underline">Contact page</Link>,
        or our <Link href="/privacy" className="text-beacon-600 hover:underline">Privacy Policy</Link> for
        more on how we handle your data generally.
      </p>
    </article>
  );
}
