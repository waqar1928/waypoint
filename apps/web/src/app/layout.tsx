import type { Metadata } from "next";
import { headers } from "next/headers";
import { Fraunces, Inter, JetBrains_Mono } from "next/font/google";
import "./globals.css";

const fraunces = Fraunces({
  variable: "--font-fraunces",
  subsets: ["latin"],
  weight: ["500", "600"],
});

const inter = Inter({
  variable: "--font-inter",
  subsets: ["latin"],
});

const jetbrainsMono = JetBrains_Mono({
  variable: "--font-jetbrains-mono",
  subsets: ["latin"],
});

const title = "Drevia | Turn your dream into your next move";
const description =
  "Drevia helps you turn ideas into clear goals, practical experiments, and next steps. Discover what you want, define why it matters, and always know what to do next.";

export const metadata: Metadata = {
  // Required for Open Graph/Twitter's relative-URL resolution and for sitemap.ts/robots.ts's
  // absolute URLs. No public asset for an og:image exists yet, so this intentionally doesn't
  // reference one — a broken image link would be worse than no image.
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3030"),
  title,
  description,
  openGraph: { title, description, type: "website", siteName: "Drevia" },
  twitter: { card: "summary", title, description },
};

export default async function RootLayout({ children }: LayoutProps<"/">) {
  // Reading headers() here is required, not decorative: it's what makes Next.js aware of the
  // per-request nonce middleware.ts generates and opts every page into dynamic rendering, which
  // is what lets Next.js apply that same nonce to its own inline hydration scripts so they match
  // the nonce in the CSP header. Without this, pages stay statically prerendered (built once,
  // with no per-request nonce at all) and the CSP blocks Next's own scripts again — see
  // middleware.ts's doc comment for what that broke in production. The nonce value itself isn't
  // used anywhere below since this app has no custom inline <script> tags of its own.
  await headers();

  return (
    <html
      lang="en"
      className={`${fraunces.variable} ${inter.variable} ${jetbrainsMono.variable} h-full antialiased`}
    >
      <body className="min-h-full flex flex-col bg-paper text-ink-900">
        {children}
      </body>
    </html>
  );
}
