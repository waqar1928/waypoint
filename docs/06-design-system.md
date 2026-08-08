# Waypoint — Design System

Brief: premium, calm, optimistic, human, intelligent, professional. Explicitly
avoiding generic SaaS-purple-gradient dashboards, corporate blue clichés,
clutter, and gamification-heavy visuals. Reference bar: Apple-level
simplicity, Linear-level product polish, Notion-level flexibility,
modern-fintech-level clarity.

## Concept

A map/compass metaphor without being literal or cheesy: warm paper-like
neutrals (the "map"), a single confident accent color used sparingly (the
"beacon" marking where to go next), and a serif display face paired with a
clean grotesque for body/UI — giving warmth without sacrificing legibility
or speed.

## Color tokens

| Token | Hex | Use |
|---|---|---|
| `paper` | `#FAF8F4` | App/page background |
| `paper-raised` | `#FFFFFF` | Cards, surfaces above background |
| `ink-900` | `#12172B` | Primary text, headings |
| `ink-700` | `#3A4256` | Secondary text |
| `ink-500` | `#6B7280` | Tertiary text, placeholders |
| `ink-300` | `#D8DAE3` | Borders, dividers |
| `ink-100` | `#EEEFF3` | Subtle fills, hover backgrounds |
| `beacon-600` | `#D9552E` | Primary accent (CTA, Next Best Action, active state) — hover |
| `beacon-500` | `#E8623D` | Primary accent — default |
| `beacon-100` | `#FBE3D8` | Primary accent tint (badges, highlight backgrounds) |
| `sage-600` | `#3F6349` | Success / Validated |
| `sage-100` | `#E4EDE6` | Success tint |
| `amber-600` | `#A66B1F` | Caution / Medium severity |
| `amber-100` | `#F5E7CF` | Caution tint |
| `merlot-600` | `#7A2A34` | Error / Invalidated / High severity |
| `merlot-100` | `#F2DEE0` | Error tint |
| `chart-900` | `#0D1226` | Dark surfaces (nav rail, footer, dark mode base) |

Contrast: `ink-900` on `paper` = 15.8:1. `beacon-500` on `paper-raised` for
text = 4.6:1 (AA for normal text); `beacon-500` is used for large text/icons
freely and paired with `paper-raised`/white text for buttons to keep body
copy on the higher-contrast `ink-900`.

## Typography

- Display/headings: **Fraunces** (serif, variable) — gives the "human,
  premium" feel without looking decorative. Used for H1–H3 and hero copy
  only.
- UI/body: **Inter** — all body text, labels, buttons, form inputs, tables.
- Monospace (code, IDs in admin): **JetBrains Mono**.

Type scale (rem, 1rem = 16px):

| Style | Size | Line height | Weight | Face |
|---|---|---|---|---|
| Display XL | 3.5 | 1.1 | 600 | Fraunces |
| Display L | 2.5 | 1.15 | 600 | Fraunces |
| H1 | 2 | 1.2 | 600 | Fraunces |
| H2 | 1.5 | 1.3 | 600 | Fraunces |
| H3 | 1.25 | 1.4 | 600 | Inter |
| Body L | 1.125 | 1.6 | 400 | Inter |
| Body | 1 | 1.6 | 400 | Inter |
| Body S | 0.875 | 1.5 | 400 | Inter |
| Caption | 0.75 | 1.4 | 500 | Inter |

## Spacing

4px base unit. Scale: `1=4px 2=8px 3=12px 4=16px 5=20px 6=24px 8=32px
10=40px 12=48px 16=64px 20=80px 24=96px`. Page gutters: 16px mobile, 24px
tablet, 40px desktop, capped content width 1200px for marketing, 1440px for
app shell.

## Radius & elevation

- Radius: buttons/inputs `10px`, cards `16px`, modals `20px`, pills `999px`.
- Elevation is mostly **hairline borders** (`1px solid ink-300`), not
  shadows — keeps the UI calm and flat. Reserve shadow for floating/overlay
  elements only: `0 8px 24px -8px rgb(18 23 43 / 0.15)`.

## Motion

- Duration: micro-interactions 120ms, panel/page transitions 220ms, easing
  `cubic-bezier(0.4, 0, 0.2, 1)`.
- Respect `prefers-reduced-motion`: disable non-essential transitions,
  keep only opacity fades under 100ms.

## Core components (states required for each: default, hover, focus,
disabled, loading, error where applicable)

- **Button**: primary (beacon fill), secondary (ink-900 outline), ghost,
  destructive (merlot). Min touch target 44×44px.
- **Card**: `paper-raised` surface, `ink-300` hairline border, `16px`
  radius, `24px` internal padding.
- **Input / Textarea / Select**: `paper-raised` bg, `ink-300` border,
  `beacon-500` focus ring (2px, offset 2px — visible focus per WCAG 2.2),
  error state swaps border/label to `merlot-600` with inline message.
- **Dialog**: centered, `20px` radius, scrim `ink-900/40%`, focus-trapped,
  closes on `Esc` and scrim click.
- **Table**: hairline row dividers, no zebra striping (calmer), sticky
  header on scroll.
- **Chart** (Momentum, viability estimate): flat fills in sage/amber/merlot,
  never 3D or gradient-heavy; always paired with a text summary for
  accessibility (no chart-only information).
- **Nav**: left rail on desktop (`chart-900` background, `paper` text) with
  the current Waypoint Arc stage always highlighted; collapses to a bottom
  tab bar under 768px.
- **Empty state**: illustration-free (no stock-y clipart) — icon + one
  sentence + a single primary action. E.g. Actions list empty: "Nothing
  scheduled yet." + "Add your next action."
- **Loading state**: skeleton blocks matching final layout shape, not
  spinners, for anything above ~300ms.
- **Error state**: inline, specific, actionable — never a bare "Something
  went wrong" without a retry action.

## Iconography

Single-weight line icons (Lucide), 1.5px stroke, 20/24px sizes only —
consistent stroke weight across the whole product, no mixed icon families.

## Accessibility commitments

- WCAG 2.2 AA color contrast on all text/icon-on-background pairs.
- All interactive elements keyboard-reachable in a logical tab order;
  visible focus ring always present (never `outline: none` without a
  replacement).
- Form errors are associated via `aria-describedby` and announced via
  `aria-live="polite"` region for async validation.
- `prefers-reduced-motion` and `prefers-color-scheme` both respected.

## Implementation

Tokens are the single source of truth in `apps/web/tailwind.config.ts`
(`theme.extend.colors/fontFamily/spacing/borderRadius`) — this document and
the config must never drift; the config is generated to match this table.
