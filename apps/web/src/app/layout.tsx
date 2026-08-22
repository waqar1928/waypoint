import type { Metadata } from "next";
import { headers } from "next/headers";
import { Fraunces, Inter, JetBrains_Mono } from "next/font/google";

import { SITE_URL } from "@/lib/site-url";
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
  "Drevia is now open for early access. Turn ideas into clear goals, practical experiments, and next steps. Discover what you want, define why it matters, and always know what to do next.";

export const metadata: Metadata = {
  // Required for Open Graph/Twitter's relative-URL resolution and for sitemap.ts/robots.ts's
  // absolute URLs.
  metadataBase: new URL(SITE_URL),
  title,
  description,
  // 1200x630 is the standard OG/Twitter card size — generated from the same compass mark used
  // for the favicon (see public/favicon.svg), not a screenshot or fabricated content.
  openGraph: {
    title,
    description,
    type: "website",
    siteName: "Drevia",
    images: [{ url: "/og-image.png", width: 1200, height: 630, alt: "Drevia" }],
  },
  twitter: { card: "summary_large_image", title, description, images: ["/og-image.png"] },
  // The Drevia mark (the compass symbol alone, no wordmark) rendered at every size browsers and
  // OSes actually request — see public/favicon.svg for the single source of truth all of these
  // are generated from. favicon.svg comes first so browsers that support vector favicons
  // (Chrome, Firefox, Edge) get a crisp icon at any pixel density; the PNGs are the fallback for
  // browsers that don't (older Safari) and for the sizes those OSes ask for by convention.
  icons: {
    icon: [
      { url: "/favicon.svg", type: "image/svg+xml" },
      { url: "/favicon-16x16.png", sizes: "16x16", type: "image/png" },
      { url: "/favicon-32x32.png", sizes: "32x32", type: "image/png" },
      { url: "/favicon-48x48.png", sizes: "48x48", type: "image/png" },
      { url: "/android-chrome-192x192.png", sizes: "192x192", type: "image/png" },
      { url: "/android-chrome-512x512.png", sizes: "512x512", type: "image/png" },
    ],
    // iOS ignores the icon[] list above and only ever looks for this — 180x180 is Apple's own
    // spec size for the largest modern device class, and iOS downsamples it for smaller ones.
    apple: [{ url: "/apple-touch-icon.png", sizes: "180x180", type: "image/png" }],
    // Legacy fallback for user agents that request /favicon.ico directly rather than reading
    // <link rel="icon">. src/app/favicon.ico is this same mark at 16/32/48 in one file.
    shortcut: ["/favicon.ico"],
  },
  // Android's "Add to Home screen" and general PWA metadata (name, theme color, 192/512 icons)
  // reads from this rather than the <link> tags above.
  manifest: "/site.webmanifest",
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
