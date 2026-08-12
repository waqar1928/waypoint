import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Privacy Policy — Waypoint",
  description: "What information Waypoint collects, why, and how it's protected.",
};

export default function PrivacyPolicyPage() {
  return (
    <article>
      <h1 className="font-display text-3xl font-semibold text-ink-900">Privacy Policy</h1>
      <p className="mt-2 text-sm text-ink-500">Last updated: 2026-08-11</p>

      <p className="mt-6 text-sm text-ink-700">
        This page describes what Waypoint actually collects and does with your information today,
        based on how the product is actually built — not a generic template. It is not a
        substitute for a review by a qualified lawyer before this product handles data from real
        users at scale, and it does not claim compliance with any specific regulatory framework
        (GDPR, CCPA, or otherwise). If you operate this product for real users, have this reviewed
        by counsel first.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">What we collect</h2>
      <ul className="mt-3 list-disc space-y-2 pl-5 text-sm text-ink-700">
        <li>
          <strong>Account information:</strong> your email address and a securely hashed password
          (we never store your password in plain text — it&apos;s hashed using ASP.NET Core
          Identity&apos;s standard algorithm before it ever touches a database).
        </li>
        <li>
          <strong>Profile information:</strong> your display name, an optional bio, an optional
          avatar image URL, and your time zone.
        </li>
        <li>
          <strong>Dream and planning content:</strong> your Dream Statement, goals, missions,
          actions, milestones, business ideas, and experiment records — the core content you
          create using the product.
        </li>
        <li>
          <strong>Journal entries:</strong> always private. No sharing mechanism exists for
          journal content anywhere in the product — only you can ever read what you write there.
        </li>
        <li>
          <strong>AI coaching conversations:</strong> the messages you exchange with Waypoint
          Coach. See our <Link href="/ai-disclosure" className="text-beacon-600 hover:underline">AI Usage Disclosure</Link> for
          exactly what&apos;s sent to our AI provider.
        </li>
        <li>
          <strong>Community and mentorship content:</strong> posts, comments, mentor profiles,
          and help requests you choose to share — visible according to the privacy level you pick
          for each post (private, community, or public).
        </li>
        <li>
          <strong>Usage and security records:</strong> login timestamps, moderation actions, and
          similar account-security events, kept in an internal audit log for accountability and
          fraud/abuse investigation. This log is not visible to other users.
        </li>
      </ul>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">What we don&apos;t do</h2>
      <ul className="mt-3 list-disc space-y-2 pl-5 text-sm text-ink-700">
        <li>We don&apos;t sell your data to anyone.</li>
        <li>We don&apos;t run third-party advertising or ad-tracking on this product.</li>
        <li>
          We don&apos;t use any product analytics or tracking tool today beyond what&apos;s
          described on this page — see our <Link href="/cookies" className="text-beacon-600 hover:underline">Cookie Policy</Link> for
          the complete list of cookies this product sets.
        </li>
      </ul>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Who we share data with</h2>
      <p className="mt-3 text-sm text-ink-700">
        When you use Waypoint Coach, the content of that specific conversation (plus limited
        context like your Dream Statement, if the conversation is about your Dream) is sent to
        Anthropic, the company that operates the AI model powering Coach, solely to generate a
        response. It is not sent for any other conversation or feature. See our{" "}
        <Link href="/ai-disclosure" className="text-beacon-600 hover:underline">AI Usage Disclosure</Link> for
        more detail. We don&apos;t share your data with any other third party.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">How your data is protected</h2>
      <ul className="mt-3 list-disc space-y-2 pl-5 text-sm text-ink-700">
        <li>Passwords are hashed, never stored or logged in plain text.</li>
        <li>
          Your session is kept in an HttpOnly cookie your browser&apos;s JavaScript can&apos;t
          read, reducing the risk of a compromised script stealing your session.
        </li>
        <li>Every account-changing request is protected against cross-site request forgery.</li>
        <li>
          Repeated failed login attempts temporarily lock an account to slow down password-
          guessing attacks.
        </li>
        <li>
          Account lockouts, content moderation actions, and account deletions are recorded in an
          internal audit trail.
        </li>
      </ul>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">How long we keep your data</h2>
      <p className="mt-3 text-sm text-ink-700">
        We keep your data for as long as your account is active. If you delete your account,
        everything tied to it — your login credentials, profile, Dream, journal, goals, actions,
        experiments, business plans, AI conversations, community posts and comments, and
        mentorship activity — is permanently removed immediately. Some records persist afterward
        regardless, for security and accountability reasons: an internal record that an account
        deletion occurred (but not its content), and system backups taken before the deletion for
        a limited retention period; see our internal{" "}
        <span className="font-mono text-xs">DISASTER_RECOVERY.md</span> document for the current
        backup retention approach.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Your choices</h2>
      <ul className="mt-3 list-disc space-y-2 pl-5 text-sm text-ink-700">
        <li>You can view and edit your profile information at any time from your account settings.</li>
        <li>
          You can permanently delete your entire account and everything in it from your account
          settings at any time.
        </li>
        <li>
          You can choose the visibility (private, community, or public) of each Community post
          individually.
        </li>
        <li>
          We don&apos;t currently offer a self-service data export tool. If you&apos;d like a copy
          of your data, <Link href="/contact" className="text-beacon-600 hover:underline">contact us</Link> and
          we&apos;ll help manually.
        </li>
      </ul>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Changes to this policy</h2>
      <p className="mt-3 text-sm text-ink-700">
        If this policy changes in a material way, we&apos;ll update the date at the top of this
        page. Continued use of Waypoint after a change means you accept the updated policy.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Questions</h2>
      <p className="mt-3 text-sm text-ink-700">
        See our <Link href="/contact" className="text-beacon-600 hover:underline">Contact page</Link> for
        how to reach us.
      </p>
    </article>
  );
}
