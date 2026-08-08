import { ButtonHTMLAttributes, forwardRef } from "react";
import { clsx } from "clsx";

type Variant = "primary" | "secondary" | "ghost" | "destructive";

const variantClasses: Record<Variant, string> = {
  primary:
    "bg-beacon-500 text-white hover:bg-beacon-600 disabled:bg-ink-300 disabled:text-ink-500",
  secondary:
    "bg-transparent text-ink-900 border border-ink-900 hover:bg-ink-100 disabled:border-ink-300 disabled:text-ink-500",
  ghost:
    "bg-transparent text-ink-700 hover:bg-ink-100 disabled:text-ink-300",
  destructive:
    "bg-merlot-600 text-white hover:opacity-90 disabled:bg-ink-300 disabled:text-ink-500",
};

export function buttonClasses(variant: Variant = "primary", className?: string) {
  return clsx(
    "inline-flex min-h-11 items-center justify-center gap-2 rounded-[10px] px-5 text-sm font-medium transition-colors duration-150 focus-visible:outline-2 focus-visible:outline-beacon-500 focus-visible:outline-offset-2 disabled:cursor-not-allowed",
    variantClasses[variant],
    className,
  );
}

export interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: Variant;
  isLoading?: boolean;
}

export const Button = forwardRef<HTMLButtonElement, ButtonProps>(
  ({ className, variant = "primary", isLoading, disabled, children, ...props }, ref) => {
    return (
      <button
        ref={ref}
        disabled={disabled || isLoading}
        aria-busy={isLoading || undefined}
        className={buttonClasses(variant, className)}
        {...props}
      >
        {isLoading ? "…" : children}
      </button>
    );
  },
);
Button.displayName = "Button";
