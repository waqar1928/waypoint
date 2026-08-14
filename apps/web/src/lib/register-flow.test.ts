import { describe, expect, it } from "vitest";
import {
  submitRegistration,
  DUPLICATE_EMAIL_MESSAGE,
  GENERIC_REGISTER_ERROR_MESSAGE,
  NETWORK_ERROR_MESSAGE,
} from "./register-flow";
import type { RegisterInput } from "./validation";

const values: RegisterInput = {
  displayName: "Alex Rivera",
  email: "alex@example.com",
  password: "GoodPass123",
};

function jsonResponse(status: number, body: unknown): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "content-type": "application/json" },
  });
}

describe("submitRegistration", () => {
  it("returns success for a 201 response", async () => {
    const outcome = await submitRegistration(
      async () => jsonResponse(201, { userId: "abc", email: values.email, emailConfirmationSent: true }),
      values,
    );
    expect(outcome).toEqual({ kind: "success" });
  });

  it("returns a duplicate-email error for a 409, using the friendly message regardless of the raw API detail", async () => {
    const outcome = await submitRegistration(
      async () =>
        jsonResponse(409, {
          type: "https://drevia.net/errors/conflict",
          title: "Conflict",
          status: 409,
          detail: "An account with this email already exists.",
          traceId: "0HNNQAF4F0FJ3:00000002",
        }),
      values,
    );
    expect(outcome).toEqual({ kind: "error", message: DUPLICATE_EMAIL_MESSAGE });
  });

  it("returns field errors for a 400 validation failure with an errors dictionary", async () => {
    const outcome = await submitRegistration(
      async () =>
        jsonResponse(400, {
          type: "https://drevia.net/errors/validation-failed",
          title: "Validation failed",
          status: 400,
          detail: "One or more fields are invalid.",
          errors: { email: ["Email is not a valid address."] },
        }),
      values,
    );
    expect(outcome).toEqual({ kind: "fieldErrors", errors: { email: "Email is not a valid address." } });
  });

  it("returns the API's detail message for a generic API failure (e.g. 500) when one is present", async () => {
    const outcome = await submitRegistration(
      async () =>
        jsonResponse(500, {
          type: "https://drevia.net/errors/unexpected",
          title: "An unexpected error occurred",
          status: 500,
        }),
      values,
    );
    // The 500 mapping in GlobalExceptionHandler.cs deliberately omits `detail` (no internal
    // error detail leaked to clients), so this exercises the "no detail available" fallback path.
    expect(outcome).toEqual({ kind: "error", message: GENERIC_REGISTER_ERROR_MESSAGE });
  });

  it("falls back to the generic message when the response body isn't valid JSON", async () => {
    const outcome = await submitRegistration(
      async () => new Response("<html>502 Bad Gateway</html>", { status: 502 }),
      values,
    );
    expect(outcome).toEqual({ kind: "error", message: GENERIC_REGISTER_ERROR_MESSAGE });
  });

  it("returns a network-error outcome instead of throwing when the request itself fails", async () => {
    const outcome = await submitRegistration(async () => {
      throw new TypeError("Failed to fetch");
    }, values);
    expect(outcome).toEqual({ kind: "error", message: NETWORK_ERROR_MESSAGE });
  });

  it("never rejects, even when the request throws — this is what keeps the caller's loading state from getting stuck", async () => {
    // The regression this whole module exists to fix: a submit handler that can throw or hang
    // leaves react-hook-form's isSubmitting stuck at true forever, since it's only reset once
    // the submit handler's promise settles. Asserting this resolves (doesn't reject) for a
    // rejecting `post` is the direct, DOM-free proof of that guarantee — see this project's
    // vitest.config.mts for why these tests deliberately avoid rendering a component.
    await expect(
      submitRegistration(async () => {
        throw new Error("connection reset");
      }, values),
    ).resolves.toEqual({ kind: "error", message: NETWORK_ERROR_MESSAGE });
  });
});
