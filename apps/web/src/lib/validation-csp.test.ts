import { describe, expect, it } from "vitest";

// Direct, executable proof that this app's actual module load order never triggers Zod's
// `new Function(...)` CSP-eval probe (see the comment at the top of validation.ts). Same
// technique Zod's own regression test uses (zod/src/v4/classic/tests/jitless-allows-eval.test.ts)
// — stub the global Function constructor to throw if invoked, then import the real code path a
// browser actually runs (validation.ts, which every page's zodResolver ultimately pulls in) and
// assert the stub was never called. This is a fresh module graph (vitest isolates per file), so
// Zod's internally-cached `allowsEval.value` hasn't been computed yet when this runs.
describe("validation.ts module load (CSP eval-probe regression)", () => {
  it("never invokes Function(...) when z.config({ jitless: true }) is set before any schema is defined", async () => {
    const originalFunction = globalThis.Function;
    let probeAttempted = false;
    // @ts-expect-error -- intentionally stubbing the global constructor for this test
    globalThis.Function = function StubFunction() {
      probeAttempted = true;
      throw new Error("Function() should never be called — this is what a strict CSP blocks in production");
    };

    try {
      // Importing validation.ts is exactly what happens when any page (register, login, etc.)
      // loads in a real browser — this is the real production code path, not a synthetic one.
      const validation = await import("./validation");
      expect(validation.registerSchema).toBeDefined();

      // Also exercise actual parsing, not just schema construction, since the fast-path setup
      // Zod's `allowsEval.value` feeds into happens when an object schema's parse machinery is
      // first built, which registerSchema.safeParse below forces if it hasn't already run.
      validation.registerSchema.safeParse({
        displayName: "Alex",
        email: "alex@example.com",
        password: "GoodPass123",
      });

      expect(probeAttempted).toBe(false);
    } finally {
      globalThis.Function = originalFunction;
    }
  });
});
