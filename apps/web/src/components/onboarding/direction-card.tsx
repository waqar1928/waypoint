import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import type { DreamDirection } from "@/lib/dream";

export function DirectionCard({
  direction,
  onSelect,
}: {
  direction: DreamDirection;
  onSelect: () => void;
}) {
  return (
    <Card className="flex h-full flex-col">
      <p className="font-display text-lg font-semibold leading-snug text-ink-900">
        {direction.directionStatement}
      </p>

      <dl className="mt-4 flex-1 space-y-3 text-sm">
        <div>
          <dt className="font-medium text-ink-700">Why this might fit</dt>
          <dd className="text-ink-500">{direction.whyItFits}</dd>
        </div>
        <div>
          <dt className="font-medium text-ink-700">Skills you already have</dt>
          <dd className="text-ink-500">{direction.skillsPresent}</dd>
        </div>
        <div>
          <dt className="font-medium text-ink-700">First experiment</dt>
          <dd className="text-ink-500">{direction.firstExperiment}</dd>
        </div>
      </dl>

      <Button onClick={onSelect} className="mt-6 w-full">
        Start with this direction
      </Button>
    </Card>
  );
}
