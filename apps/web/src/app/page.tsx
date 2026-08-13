import Link from "next/link";
import {
  Compass,
  MessageCircleQuestion,
  FlaskConical,
  Building2,
  LineChart,
  ArrowRight,
} from "lucide-react";
import { SiteHeader } from "@/components/marketing/site-header";
import { SiteFooter } from "@/components/marketing/site-footer";
import { Card } from "@/components/ui/card";
import { buttonClasses } from "@/components/ui/button";

const arcStages = [
  { name: "Discover", description: "Conversational prompts surface your interests, strengths, and possible directions." },
  { name: "Define", description: "Turn a direction into a clear Dream Statement: purpose, audience, and problem." },
  { name: "Validate", description: "Name your obstacles and test assumptions cheaply before you commit." },
  { name: "Plan", description: "Cascade the dream into a 90-day mission, a 30-day goal, and a next action." },
  { name: "Act", description: "A dream-native task system that always shows your Next Best Action." },
];

const features = [
  {
    icon: MessageCircleQuestion,
    title: "Drevia Coach",
    description:
      "An AI thinking partner that asks useful questions, challenges assumptions, and helps you break big goals into a next small step. Not empty motivation.",
  },
  {
    icon: Compass,
    title: "Dream Planner",
    description:
      "Cascade a dream from a 5-year vision down to the one thing you should do today, with every level editable by you.",
  },
  {
    icon: FlaskConical,
    title: "Experiment Lab",
    description:
      "Test an idea with a small, cheap experiment instead of a big commitment. Idea → Experiment → Result → Learning → Next experiment.",
  },
  {
    icon: Building2,
    title: "Business Builder",
    description:
      "A focused workspace for business-shaped dreams: customer, problem, value proposition, pricing, risks. Built for validation, not a 40-page plan.",
  },
  {
    icon: LineChart,
    title: "Momentum",
    description:
      "Track actions taken, conversations had, and lessons learned, not just tasks checked off. Progress you can actually see.",
  },
];

const faqs = [
  {
    question: "Is Drevia affiliated with any book or author?",
    answer:
      "No. Drevia is an original product with its own terminology, questions, and exercises. It is not affiliated with, endorsed by, or associated with any author, book, or personal brand.",
  },
  {
    question: "Does the AI just tell me what to do?",
    answer:
      "No. Drevia Coach asks questions, surfaces blind spots, and suggests small experiments. Decisions stay yours. Any AI-generated estimate is clearly labeled as decision support, not a guarantee.",
  },
  {
    question: "Do I need a business idea to use Drevia?",
    answer:
      "No. Most people start with \"I don't know what I want to do.\" Discovery and Definition come before anything business-shaped, and plenty of dreams aren't businesses at all.",
  },
  {
    question: "What does it cost?",
    answer: "Pricing will be announced before general availability.",
  },
];

export default function LandingPage() {
  return (
    <>
      <SiteHeader />
      <main id="main">
        <section className="mx-auto max-w-6xl px-4 pb-20 pt-16 sm:px-6 sm:pt-24 lg:px-10">
          <div className="mx-auto max-w-3xl text-center">
            <p className="mb-4 inline-flex items-center gap-2 rounded-full bg-beacon-100 px-4 py-1.5 text-sm font-medium text-beacon-600">
              <Compass className="h-4 w-4" aria-hidden="true" />
              Discover → Define → Validate → Plan → Act
            </p>
            <h1 className="font-display text-4xl font-semibold leading-tight text-ink-900 sm:text-5xl lg:text-6xl">
              You already have a dream. Let&rsquo;s find it.
            </h1>
            <p className="mx-auto mt-6 max-w-xl text-lg text-ink-700">
              Turn the thing you&rsquo;ve been thinking about into something you can actually do.
            </p>
            <div className="mt-10 flex flex-col items-center justify-center gap-4 sm:flex-row">
              <Link href="/register" className={buttonClasses("primary", "px-8 text-base")}>
                Find My Dream
                <ArrowRight className="h-4 w-4" aria-hidden="true" />
              </Link>
              <Link href="#how-it-works" className={buttonClasses("secondary", "px-8 text-base")}>
                See How It Works
              </Link>
            </div>
          </div>
        </section>

        <section id="how-it-works" className="border-t border-ink-300 bg-paper-raised py-20">
          <div className="mx-auto max-w-6xl px-4 sm:px-6 lg:px-10">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="font-display text-3xl font-semibold text-ink-900">
                How Drevia works
              </h2>
              <p className="mt-4 text-ink-700">
                Every feature attaches to one stage of this arc, so you always know where you are
                and what comes next.
              </p>
            </div>
            <ol className="mx-auto mt-14 grid max-w-5xl gap-6 sm:grid-cols-2 lg:grid-cols-5">
              {arcStages.map((stage, index) => (
                <li key={stage.name}>
                  <Card className="h-full">
                    <span className="font-display text-sm text-beacon-600">
                      {String(index + 1).padStart(2, "0")}
                    </span>
                    <h3 className="mt-2 font-display text-lg font-semibold text-ink-900">
                      {stage.name}
                    </h3>
                    <p className="mt-2 text-sm text-ink-700">{stage.description}</p>
                  </Card>
                </li>
              ))}
            </ol>
          </div>
        </section>

        <section id="features" className="py-20">
          <div className="mx-auto max-w-6xl px-4 sm:px-6 lg:px-10">
            <div className="mx-auto max-w-2xl text-center">
              <h2 className="font-display text-3xl font-semibold text-ink-900">
                Everything you need between &ldquo;I don&rsquo;t know&rdquo; and &ldquo;here&rsquo;s my plan&rdquo;
              </h2>
            </div>
            <div className="mt-14 grid gap-6 sm:grid-cols-2 lg:grid-cols-3">
              {features.map((feature) => (
                <Card key={feature.title}>
                  <feature.icon className="h-6 w-6 text-beacon-500" aria-hidden="true" />
                  <h3 className="mt-4 font-display text-lg font-semibold text-ink-900">
                    {feature.title}
                  </h3>
                  <p className="mt-2 text-sm text-ink-700">{feature.description}</p>
                </Card>
              ))}
            </div>
          </div>
        </section>

        <section className="border-y border-ink-300 bg-chart-900 py-20 text-paper">
          <div className="mx-auto max-w-3xl px-4 text-center sm:px-6 lg:px-10">
            <p className="font-display text-2xl italic leading-relaxed sm:text-3xl">
              &ldquo;Testimonials from early users will go here once Drevia is in
              private beta.&rdquo;
            </p>
            <p className="mt-4 text-sm text-ink-300">Placeholder, not a real testimonial</p>
          </div>
        </section>

        <section id="faq" className="py-20">
          <div className="mx-auto max-w-3xl px-4 sm:px-6 lg:px-10">
            <h2 className="text-center font-display text-3xl font-semibold text-ink-900">
              Frequently asked questions
            </h2>
            <dl className="mt-12 space-y-8">
              {faqs.map((faq) => (
                <div key={faq.question} className="border-b border-ink-300 pb-8">
                  <dt className="font-display text-lg font-semibold text-ink-900">
                    {faq.question}
                  </dt>
                  <dd className="mt-2 text-ink-700">{faq.answer}</dd>
                </div>
              ))}
            </dl>
          </div>
        </section>

        <section className="pb-24">
          <div className="mx-auto max-w-3xl rounded-2xl bg-beacon-500 px-8 py-16 text-center text-white sm:px-16">
            <h2 className="font-display text-3xl font-semibold sm:text-4xl">
              What should you do today?
            </h2>
            <p className="mt-4 text-beacon-100">
              Start with one conversation. Drevia can help you think it through.
            </p>
            <Link
              href="/register"
              className="mt-8 inline-flex min-h-11 items-center justify-center rounded-[10px] bg-white px-8 text-base font-medium text-beacon-600 transition-colors hover:bg-beacon-100 focus-visible:outline-2 focus-visible:outline-white focus-visible:outline-offset-2"
            >
              Find My Dream
            </Link>
          </div>
        </section>
      </main>
      <SiteFooter />
    </>
  );
}
