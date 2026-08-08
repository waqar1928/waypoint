import { InputHTMLAttributes, LabelHTMLAttributes, forwardRef, useId } from "react";
import { clsx } from "clsx";

export const Input = forwardRef<HTMLInputElement, InputHTMLAttributes<HTMLInputElement>>(
  ({ className, "aria-invalid": ariaInvalid, ...props }, ref) => (
    <input
      ref={ref}
      aria-invalid={ariaInvalid}
      className={clsx(
        "min-h-11 w-full rounded-[10px] border bg-paper-raised px-3.5 text-sm text-ink-900 placeholder:text-ink-500 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2",
        ariaInvalid ? "border-merlot-600" : "border-ink-300",
        className,
      )}
      {...props}
    />
  ),
);
Input.displayName = "Input";

export function Label({ className, ...props }: LabelHTMLAttributes<HTMLLabelElement>) {
  return (
    <label
      className={clsx("mb-1.5 block text-sm font-medium text-ink-900", className)}
      {...props}
    />
  );
}

export function FieldError({ children, id }: { children?: string; id: string }) {
  if (!children) return null;
  return (
    <p id={id} role="alert" className="mt-1.5 text-sm text-merlot-600">
      {children}
    </p>
  );
}

export function useFieldIds(name: string) {
  const uniqueId = useId();
  return {
    inputId: `${name}-${uniqueId}`,
    errorId: `${name}-${uniqueId}-error`,
  };
}
