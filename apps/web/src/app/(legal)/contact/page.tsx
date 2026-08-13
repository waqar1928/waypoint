import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Contact | Drevia",
  description: "How to reach Drevia for support, privacy questions, or anything else.",
};

// This address matches the real production domain, but the inbox itself hasn't been set up and
// monitored yet. Whoever operates this product for real needs to confirm it's live and checked
// regularly before this page can be trusted for anything time-sensitive (see
// docs/PRODUCTION_READINESS_AUDIT.md's Legal/Trust section).
const SUPPORT_EMAIL = "support@drevia.net";

export default function ContactPage() {
  return (
    <article>
      <h1 className="font-display text-3xl font-semibold text-ink-900">Contact</h1>

      <p className="mt-6 text-sm text-ink-700">
        Have a question, a privacy request, a bug report, or a content concern? Reach out and
        we&apos;ll get back to you.
      </p>

      <div className="mt-6 rounded-[10px] border border-dashed border-ink-300 bg-paper-raised p-4">
        <p className="text-sm text-ink-700">
          <strong>[Placeholder, replace before real launch]</strong> This address hasn&apos;t been
          set up as a monitored inbox yet. Whoever operates this product for real needs to confirm
          it&apos;s live before this page goes out to actual users.
        </p>
      </div>

      <p className="mt-6 text-sm text-ink-700">
        Email us at{" "}
        <a href={`mailto:${SUPPORT_EMAIL}`} className="text-beacon-600 hover:underline">
          {SUPPORT_EMAIL}
        </a>
        . For account deletion or data requests, please mention that in your message so we can
        prioritize it appropriately.
      </p>

      <p className="mt-6 text-sm text-ink-700">
        For most account actions, like updating your profile, deleting your account, or changing
        what a post is visible to, you don&apos;t need to contact us at all. Those are all
        self-service from your account settings.
      </p>
    </article>
  );
}
