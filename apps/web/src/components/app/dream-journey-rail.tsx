import Link from "next/link";
import { clsx } from "clsx";
import { CheckCircle2, CircleDot, CircleDashed, Circle } from "lucide-react";
import { Card } from "@/components/ui/card";
import type { JourneyNode, JourneyNodeState } from "@/lib/journey";

const stateIcon: Record<JourneyNodeState, typeof CheckCircle2> = {
  completed: CheckCircle2,
  active: CircleDot,
  upcoming: CircleDashed,
  empty: Circle,
};

// Every state pairs a distinct icon SHAPE with a color - never color alone - and is always
// paired with the node's visible stateLabel text below, so screen reader and low-vision/color-
// blind users get the same information as anyone reading the dot.
const stateIconClasses: Record<JourneyNodeState, string> = {
  completed: "text-sage-600",
  active: "text-beacon-500",
  upcoming: "text-amber-600",
  empty: "text-ink-300",
};

const stateTagClasses: Record<JourneyNodeState, string> = {
  completed: "bg-sage-100 text-sage-600",
  active: "bg-beacon-100 text-beacon-600",
  upcoming: "bg-amber-100 text-amber-600",
  empty: "bg-ink-100 text-ink-500",
};

/**
 * A compact, single-column summary of the whole Dream -> Vision -> Direction -> Goal -> Mission
 * -> Next Move -> Experiment -> Learning sequence, composed entirely from data the Dream Overview
 * page already fetches (see lib/journey.ts). This is a navigation/summary layer over the existing
 * detail cards below - it doesn't replace them, and every node with an href points at the actual
 * place that data is edited or the existing card that already shows it in full.
 */
export function DreamJourneyRail({ nodes }: { nodes: JourneyNode[] }) {
  return (
    <Card>
      <h2 className="font-display text-base font-semibold text-ink-900">Your journey</h2>
      <p className="mt-1 text-sm text-ink-500">A quick look at where this dream stands, start to finish.</p>

      <nav aria-label="Dream journey" className="mt-5">
        <ol className="relative border-l-2 border-ink-300 pl-6">
          {nodes.map((node) => {
            const Icon = stateIcon[node.state];
            const body = (
              <>
                <span className="flex flex-wrap items-center gap-2">
                  <span className="text-xs font-semibold uppercase tracking-wide text-ink-700">{node.label}</span>
                  <span
                    className={clsx(
                      "rounded-full px-2 py-0.5 text-[11px] font-medium",
                      stateTagClasses[node.state],
                    )}
                  >
                    {node.stateLabel}
                  </span>
                </span>
                <span className={clsx("mt-1 block text-sm", node.summary ? "text-ink-900" : "text-ink-500")}>
                  {node.summary ?? node.placeholder}
                </span>
              </>
            );

            return (
              <li key={node.key} className="relative pb-6 last:pb-0">
                <span
                  aria-hidden="true"
                  className={clsx(
                    "absolute -left-[calc(1.5rem+9px)] top-0.5 rounded-full bg-paper-raised",
                    stateIconClasses[node.state],
                  )}
                >
                  <Icon className="h-4 w-4" />
                </span>
                {node.href ? (
                  <Link href={node.href} className="block rounded-md">
                    {body}
                  </Link>
                ) : (
                  <div>{body}</div>
                )}
              </li>
            );
          })}
        </ol>
      </nav>
    </Card>
  );
}
