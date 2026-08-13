import type { Metadata } from "next";
import Link from "next/link";

export const metadata: Metadata = {
  title: "Terms of Service | Drevia",
  description: "The terms that govern your use of Drevia.",
};

export default function TermsOfServicePage() {
  return (
    <article>
      <h1 className="font-display text-3xl font-semibold text-ink-900">Terms of Service</h1>
      <p className="mt-2 text-sm text-ink-500">Last updated: 2026-08-11</p>

      <p className="mt-6 text-sm text-ink-700">
        These terms describe the actual rules and behavior of this product as built. They are a
        starting point, not a substitute for review by a qualified lawyer before this product
        serves real users at scale. In particular,{" "}
        <strong>the governing-law and jurisdiction section below needs to be filled in</strong>{" "}
        by whoever operates this product for real. It&apos;s deliberately left as a placeholder
        rather than an invented jurisdiction, since that&apos;s a real legal decision, not a
        technical one.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">1. What Drevia is</h2>
      <p className="mt-3 text-sm text-ink-700">
        Drevia is a planning and accountability tool that helps you turn a personal or business
        dream into a concrete plan, including an AI coaching feature, community features, and
        mentorship features. Drevia is not affiliated with, endorsed by, or associated with any
        author, book, or personal brand.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">2. Your account</h2>
      <ul className="mt-3 list-disc space-y-2 pl-5 text-sm text-ink-700">
        <li>You must provide an accurate email address and keep your password secure.</li>
        <li>You&apos;re responsible for all activity that happens under your account.</li>
        <li>You must be old enough, under the laws that apply to you, to enter into this agreement.</li>
        <li>
          We may lock or terminate accounts that violate these terms, including for content that
          gets reported and confirmed to violate our community guidelines (see Section 4).
        </li>
      </ul>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">3. Your content</h2>
      <p className="mt-3 text-sm text-ink-700">
        You own what you create in Drevia: your Dream Statement, journal entries, goals,
        business plans, community posts, and everything else you write. By posting content with a
        visibility level other than &quot;private,&quot; you grant Drevia a license to display
        that content to the audience you chose (other Drevia members, for &quot;community&quot;
        or &quot;public&quot; visibility). We don&apos;t claim ownership of your content, and we
        don&apos;t use it to train AI models.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">4. Acceptable use</h2>
      <p className="mt-3 text-sm text-ink-700">Don&apos;t use Drevia to:</p>
      <ul className="mt-3 list-disc space-y-2 pl-5 text-sm text-ink-700">
        <li>Post spam, harassment, or content that violates someone else&apos;s rights.</li>
        <li>Impersonate another person or misrepresent your affiliation with anyone.</li>
        <li>Attempt to access another user&apos;s account or data without authorization.</li>
        <li>Interfere with or disrupt the service, including by attempting to bypass rate limits or security controls.</li>
      </ul>
      <p className="mt-3 text-sm text-ink-700">
        Content in Community and Mentorship features can be reported by any member. Reports are
        reviewed by moderators, who can dismiss a report, remove the content, or take other
        action, all of which is recorded in an internal audit trail.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">5. Drevia Coach (AI)</h2>
      <p className="mt-3 text-sm text-ink-700">
        Drevia Coach is an AI assistant, not a human advisor, and not a licensed professional of
        any kind. Its responses are not guaranteed to be accurate, complete, or suitable for your
        situation, and should not be treated as professional business, financial, legal, tax, or
        medical advice. See our{" "}
        <Link href="/ai-disclosure" className="text-beacon-600 hover:underline">AI Usage Disclosure</Link> for
        details on how it works and what data it uses.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">6. No guarantee of outcomes</h2>
      <p className="mt-3 text-sm text-ink-700">
        Drevia is a planning and accountability tool. It does not guarantee that you&apos;ll
        achieve your dream, validate your business idea, or reach any particular outcome. Your
        results depend on you.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">7. Termination</h2>
      <p className="mt-3 text-sm text-ink-700">
        You can delete your own account at any time from your account settings. We may suspend or
        terminate accounts that violate these terms.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">8. Disclaimer and limitation of liability</h2>
      <p className="mt-3 text-sm text-ink-700">
        Drevia is provided &quot;as is,&quot; without warranties of any kind, express or
        implied. To the fullest extent permitted by law, Drevia and its operators aren&apos;t
        liable for any indirect, incidental, or consequential damages arising from your use of the
        service.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">9. Governing law</h2>
      <p className="mt-3 rounded-[10px] border border-dashed border-ink-300 bg-paper-raised p-4 text-sm text-ink-700">
        <strong>[Placeholder, fill in before real launch]</strong> These terms are governed by
        the laws of [jurisdiction], without regard to conflict-of-law principles. Any dispute will
        be resolved in the courts of [jurisdiction]. This section requires an actual decision by
        whoever operates this product about where it&apos;s legally based. It&apos;s
        intentionally not filled in with an invented answer.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">10. Changes to these terms</h2>
      <p className="mt-3 text-sm text-ink-700">
        If these terms change in a material way, we&apos;ll update the date at the top of this
        page. Continued use of Drevia after a change means you accept the updated terms.
      </p>

      <h2 className="mt-8 font-display text-xl font-semibold text-ink-900">Questions</h2>
      <p className="mt-3 text-sm text-ink-700">
        See our <Link href="/contact" className="text-beacon-600 hover:underline">Contact page</Link> for
        how to reach us.
      </p>
    </article>
  );
}
