import type { Metadata } from "next";

export const metadata: Metadata = {
  title: "Contact | Drevia",
  description: "How to reach Drevia for support, privacy questions, or anything else.",
};

// Verified as a real, actively monitored inbox before this page went out to real users.
const SUPPORT_EMAIL = "support@drevia.net";

export default function ContactPage() {
  return (
    <article>
      <h1 className="font-display text-3xl font-semibold text-ink-900">Contact</h1>

      <p className="mt-6 text-sm text-ink-700">
        Have a question, a privacy request, a bug report, or a content concern? Reach out and
        we&apos;ll get back to you.
      </p>

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
