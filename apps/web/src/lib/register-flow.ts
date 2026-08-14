import { isProblemDetails } from "./api-types";
import type { RegisterInput } from "./validation";

export type RegisterOutcome =
  | { kind: "success" }
  | { kind: "fieldErrors"; errors: Partial<Record<keyof RegisterInput, string>> }
  | { kind: "error"; message: string };

export const DUPLICATE_EMAIL_MESSAGE =
  "An account with this email already exists. Please log in or use a different email address.";
export const GENERIC_REGISTER_ERROR_MESSAGE = "We couldn't create your account. Please try again.";
export const NETWORK_ERROR_MESSAGE = "We couldn't reach Drevia. Check your connection and try again.";

/**
 * Submits the registration form and always resolves to a RegisterOutcome — never throws, never
 * rejects. That guarantee is what actually fixes the "stuck on submit" bug: the caller
 * (register/page.tsx) just awaits this and sets state from the result, so react-hook-form's
 * isSubmitting always flips back to false once this settles, regardless of what went wrong
 * (a bad status code, a malformed response body, or the fetch itself failing over a broken
 * connection).
 *
 * `post` is injected (rather than importing apiMutate directly) so this stays a pure function
 * per this project's testing convention (see vitest.config.mts) — no network, no DOM, unit
 * tests just pass in a fake that returns/throws whatever a real fetch could.
 */
export async function submitRegistration(
  post: (values: RegisterInput) => Promise<Response>,
  values: RegisterInput,
): Promise<RegisterOutcome> {
  let response: Response;
  try {
    response = await post(values);
  } catch {
    return { kind: "error", message: NETWORK_ERROR_MESSAGE };
  }

  if (response.ok) {
    return { kind: "success" };
  }

  const payload = await response.json().catch(() => null);

  // The register endpoint only ever throws ConflictException for one reason (see
  // RegisterUserCommandHandler.cs) — a duplicate email — so 409 here always means that specific,
  // known case. Show the actionable message regardless of the API's raw `detail` text, rather
  // than surfacing backend wording verbatim.
  if (response.status === 409) {
    return { kind: "error", message: DUPLICATE_EMAIL_MESSAGE };
  }

  if (isProblemDetails(payload) && payload.errors) {
    const errors: Partial<Record<keyof RegisterInput, string>> = {};
    for (const [field, messages] of Object.entries(payload.errors)) {
      errors[field as keyof RegisterInput] = messages[0];
    }
    return { kind: "fieldErrors", errors };
  }

  return {
    kind: "error",
    message: isProblemDetails(payload) && payload.detail ? payload.detail : GENERIC_REGISTER_ERROR_MESSAGE,
  };
}
