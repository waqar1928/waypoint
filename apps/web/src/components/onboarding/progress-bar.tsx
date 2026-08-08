export function ProgressBar({ step, total, label }: { step: number; total: number; label: string }) {
  const percent = Math.round((step / total) * 100);

  return (
    <div className="mb-8">
      <div className="mb-2 flex items-center justify-between text-sm text-ink-500">
        <span>{label}</span>
        <span>
          Step {step} of {total}
        </span>
      </div>
      <div
        role="progressbar"
        aria-valuenow={step}
        aria-valuemin={0}
        aria-valuemax={total}
        className="h-1.5 w-full overflow-hidden rounded-full bg-ink-100"
      >
        <div
          className="h-full rounded-full bg-beacon-500 transition-all duration-300"
          style={{ width: `${percent}%` }}
        />
      </div>
    </div>
  );
}
