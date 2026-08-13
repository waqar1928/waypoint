import { describe, expect, it } from "vitest";
import { isProblemDetails } from "./api-types";

describe("isProblemDetails", () => {
  it("recognizes a well-formed RFC 7807 problem details body", () => {
    expect(
      isProblemDetails({
        type: "https://drevia.net/errors/validation-failed",
        title: "Validation failed",
        status: 400,
      }),
    ).toBe(true);
  });

  it("recognizes a minimal object with only title and status", () => {
    expect(isProblemDetails({ title: "Not found", status: 404 })).toBe(true);
  });

  it("rejects null", () => {
    expect(isProblemDetails(null)).toBe(false);
  });

  it("rejects undefined", () => {
    expect(isProblemDetails(undefined)).toBe(false);
  });

  it("rejects a plain string", () => {
    expect(isProblemDetails("Not found")).toBe(false);
  });

  it("rejects an object missing status", () => {
    expect(isProblemDetails({ title: "Not found" })).toBe(false);
  });

  it("rejects an object missing title", () => {
    expect(isProblemDetails({ status: 404 })).toBe(false);
  });

  it("rejects an array", () => {
    expect(isProblemDetails([])).toBe(false);
  });
});
