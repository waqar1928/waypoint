import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "AI Usage Disclosure | Drevia",
  description: "How Drevia Coach works, what it does with your data, and its real limitations.",
};

export default function AiDisclosurePage() {
  return (
    <article>
      <h1 className="font-display text-3xl font-semibold text-ink-900">AI Usage Disclosure</h1>
      <p className="mt-2 text-sm text-ink-500">Last updated: 2026-08-11</p>

      <p className="mt-6 text-sm text-ink-700">
        Drevia Coach is the only AI feature in this product. This page describes, plainly and
        accurately, what it is, what it isn&apos;t, and what happens to what you type into it.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">You&apos;re talking to an AI, not a person</h2>
      <p className="mt-3 text-sm text-ink-700">
        Drevia Coach is powered by an AI language model (Anthropic&apos;s Claude). It is not a
        human coach, mentor, or advisor, and it doesn&apos;t have a persistent memory of you
        beyond the specific conversation you&apos;re in. Each conversation only knows what&apos;s
        been said within it, plus, in some conversation types, a summary of your own Dream
        Statement or business idea that you wrote yourself.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Its answers aren&apos;t guaranteed to be right</h2>
      <p className="mt-3 text-sm text-ink-700">
        Like any AI language model, Drevia Coach can be wrong, incomplete, or miss important
        context about your specific situation. Its responses are not professional business,
        financial, legal, tax, or medical advice, and shouldn&apos;t be treated as a substitute
        for a qualified professional when the stakes are real. Use your own judgment.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">What gets sent, and to whom</h2>
      <p className="mt-3 text-sm text-ink-700">
        When you send a message to Coach, that message, plus the recent history of that specific
        conversation, plus (depending on the conversation type) a short summary of your Dream
        Statement or business idea, is sent to Anthropic&apos;s API to generate a response.
        Nothing else about your account (your password, other conversations, journal entries,
        community activity, etc.) is ever included. Your message doesn&apos;t get appended to or
        merged with any system-level instructions in a way that could be used to hijack the
        assistant&apos;s behavior. It&apos;s always sent as your own message, clearly separated
        from the assistant&apos;s instructions.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Content moderation</h2>
      <p className="mt-3 text-sm text-ink-700">
        Today, Coach&apos;s output moderation is basic. It checks whether a response came back
        empty or blocked, and shows you a clear message if so, rather than a broken empty
        response. It does not yet run a dedicated content-safety review on every AI response. If
        Coach ever produces something inappropriate or wrong, please don&apos;t treat it as an
        authoritative answer, and let us know via our{" "}
        <Link href="/contact" className="text-beacon-600 hover:underline">Contact page</Link>.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Limits on how much you can use it</h2>
      <p className="mt-3 text-sm text-ink-700">
        To keep the service reliable and fairly available to everyone, Coach is rate-limited per
        account, and any single conversation has a maximum length. Once reached, you&apos;ll be
        asked to start a new conversation to continue.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Your conversations are stored</h2>
      <p className="mt-3 text-sm text-ink-700">
        Your Coach conversations are saved to your account, same as any other Drevia content, so
        you can revisit them later. They&apos;re private to you. See our{" "}
        <Link href="/privacy" className="text-beacon-600 hover:underline">Privacy Policy</Link> for
        how we handle your data generally.
      </p>
    </article>
  );
}
