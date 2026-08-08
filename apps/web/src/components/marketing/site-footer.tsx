import Link from "next/link";

export function SiteFooter() {
  return (
    <footer className="mt-auto border-t border-ink-300 bg-paper">
      <div className="mx-auto flex max-w-6xl flex-col gap-4 px-4 py-10 text-sm text-ink-500 sm:flex-row sm:items-center sm:justify-between sm:px-6 lg:px-10">
        <p>
          &copy; {new Date().getFullYear()} Waypoint. Not affiliated with, endorsed by, or
          associated with any author, book, or personal brand.
        </p>
        <nav aria-label="Footer" className="flex gap-6">
          <Link href="/#faq" className="hover:text-ink-900">FAQ</Link>
          <Link href="/login" className="hover:text-ink-900">Log in</Link>
        </nav>
      </div>
    </footer>
  );
}
