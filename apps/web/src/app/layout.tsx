import type { Metadata } from "next";
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

const title = "Waypoint — Turn your dream into a plan you can act on";
const description =
  "Waypoint is the operating system for turning dreams into action: discover what you want, define why it matters, validate it cheaply, and always know your next best step.";

export const metadata: Metadata = {
  // Required for Open Graph/Twitter's relative-URL resolution and for sitemap.ts/robots.ts's
  // absolute URLs — previously unset entirely (see docs/PRODUCTION_READINESS_AUDIT.md's SEO
  // section). No public asset for an og:image exists yet, so this intentionally doesn't reference
  // one — a broken image link would be worse than no image.
  metadataBase: new URL(process.env.NEXT_PUBLIC_SITE_URL ?? "http://localhost:3030"),
  title,
  description,
  openGraph: { title, description, type: "website", siteName: "Waypoint" },
  twitter: { card: "summary", title, description },
};

export default function RootLayout({ children }: LayoutProps<"/">) {
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
