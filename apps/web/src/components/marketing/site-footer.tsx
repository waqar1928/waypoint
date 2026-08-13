import Link from "next/link";

export function SiteFooter() {
  return (
    <footer className="mt-auto border-t border-ink-300 bg-paper">
      <div className="mx-auto flex max-w-6xl flex-col gap-4 px-4 py-10 text-sm text-ink-500 sm:px-6 lg:px-10">
        <div className="flex flex-col gap-4 sm:flex-row sm:items-center sm:justify-between">
          <p>
            &copy; {new Date().getFullYear()} Drevia. Not affiliated with, endorsed by, or
            associated with any author, book, or personal brand.
          </p>
          <nav aria-label="Footer" className="flex gap-6">
            <Link href="/#faq" className="hover:text-ink-900">FAQ</Link>
            <Link href="/login" className="hover:text-ink-900">Log in</Link>
          </nav>
        </div>
        <nav aria-label="Legal" className="flex flex-wrap gap-x-6 gap-y-2 border-t border-ink-300 pt-4">
          <Link href="/privacy" className="hover:text-ink-900">Privacy Policy</Link>
          <Link href="/terms" className="hover:text-ink-900">Terms of Service</Link>
          <Link href="/cookies" className="hover:text-ink-900">Cookie Policy</Link>
          <Link href="/ai-disclosure" className="hover:text-ink-900">AI Usage Disclosure</Link>
          <Link href="/contact" className="hover:text-ink-900">Contact</Link>
        </nav>
      </div>
    </footer>
  );
}
