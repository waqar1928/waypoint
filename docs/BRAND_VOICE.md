# Drevia: Brand Voice

This document exists so anyone writing copy for Drevia (a person or an AI
assistant) can match the voice that's already established across the
product, without having to reverse-engineer it from example pages every
time.

## Brand name

**Drevia.** Not "DREVIA," not "Drevia App," not "the Drevia platform" (just
"Drevia" is usually enough). Say the name once per page or section, not in
every sentence. If a sentence reads fine without it, leave it out.

## Positioning

Drevia is a planning and accountability tool. It helps someone go from "I
have an idea I can't stop thinking about" to a concrete next step, through
four moves: discover what you actually want, define it clearly, test it
cheaply before committing, and act with a next action that's always
obvious.

Drevia is an original product. It's not affiliated with, endorsed by, or
derived from any specific author, book, or personal brand, and copy should
never imply otherwise.

## Tone

Clear, warm, direct, intelligent, conversational, confident, practical,
human. Write like a thoughtful person talking to another person, not like a
brand talking at a customer.

**Core principle: write like a smart, thoughtful human. Be useful before
being impressive.**

## Writing principles

- **Short sentences over long ones.** If a sentence needs a semicolon or
  three clauses, it's probably two sentences.
- **Plain punctuation over long dashes.** No em dashes (—), no en dashes
  (–) in anything a user reads. Use a period, a comma, or rewrite the
  sentence. (Hyphens in compound words, like "password-guessing," are
  fine. It's specifically the long dash used as a sentence-connector that's
  off-limits.)
- **Don't over-explain simple things.** If a button says "Save," it doesn't
  need a tooltip explaining what saving does.
- **Not every sentence needs to be motivational.** Most copy in this
  product is functional: it tells someone what happened, what to do next,
  or what something means. Save warmth for where it actually helps (error
  recovery, empty states, the AI coach), not as a default seasoning on
  every sentence.
- **Ask, don't declare, when you don't know.** The AI coach in particular
  should surface questions and name uncertainty rather than hand down
  verdicts. "That could work. Here's what I'd check first" beats "You're
  absolutely on the right path!"
- **Say what actually happened.** Error messages describe the real
  problem and, when possible, how to fix it. Never expose a raw exception
  or a technical stack trace to a user.
- **Don't promise what isn't built.** Copy describes real features. If
  something is a placeholder or a future idea, say so plainly rather than
  implying it works today.

## Words and phrases to avoid

These read as generic, AI-generated marketing copy, and none of them
appear anywhere in this product on purpose:

unlock your potential · embark on your journey · transform your dreams ·
empower yourself · unleash your potential · your journey starts here ·
discover the power within · achieve your goals effortlessly · turn your
aspirations into reality · make your dreams come true · supercharge your
success · unlock a world of possibilities · seamlessly · game-changing ·
revolutionize · limitless · next-level · elevate · holistic · innovative
solution · cutting-edge · actionable insights · meaningful impact

If a sentence would only make sense in a SaaS landing-page template, rewrite
it or cut it.

## Words to use

Plain, concrete verbs and nouns: turn, build, try, test, decide, next
move, next step, working on, figure out, worth trying. Say "dream" for the
big thing someone's working toward, and be specific about the mechanism
(goal, action, experiment, conversation) rather than reaching for a vaguer
word.

## Examples

**Bad:**
"Embark on a transformative journey to unlock your full potential and turn
your aspirations into meaningful achievements."

**Good:**
"Figure out what you want to do, then decide what to try next."

---

**Bad:**
"Leverage our powerful AI-driven platform to unlock unprecedented clarity
and accelerate your journey toward success."

**Good:**
"Not sure what to do next? Drevia Coach can help you think it through."

---

**Bad (AI coach):**
"That's an incredible dream! You're taking an amazing first step toward
unlocking your limitless potential!"

**Good (AI coach):**
"That's interesting. Before you build anything, let's figure out who would
actually want it."

---

**Bad (button):**
"Unlock Your Potential" / "Begin Your Transformation"

**Good (button):**
"Continue" / "Save" / "Try this" / "Talk to Coach"

---

**Bad (empty state):**
"Nothing here yet."

**Good (empty state):**
"You haven't created a dream yet. Start with the idea that's been on your
mind."

---

**Bad (error):**
"An unexpected error occurred." / "Validation failed."

**Good (error):**
"Something went wrong. Please try again." / "Please enter a title before
saving."

## Where this shows up

This voice applies everywhere a real person reads Drevia's words: the
landing page, onboarding questions, dashboard copy, buttons and microcopy,
empty states, error messages, emails, the AI coach's responses, and the
legal/trust pages (privacy, terms, cookies, AI disclosure, contact). It
does not apply to internal code comments, log messages, or developer
documentation, which can be as technical as they need to be.
