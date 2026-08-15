import { describe, expect, it } from "vitest";
import {
  becomeMentorSchema,
  createHelpRequestSchema,
  createPostSchema,
  dreamStatementSchema,
  forgotPasswordSchema,
  loginSchema,
  profileSchema,
  registerSchema,
  reportContentSchema,
  resetPasswordSchema,
} from "./validation";

describe("registerSchema", () => {
  const valid = { displayName: "Alex Rivera", email: "alex@example.com", password: "GoodPass123" };

  it("accepts a valid registration", () => {
    expect(registerSchema.safeParse(valid).success).toBe(true);
  });

  it("rejects an empty display name", () => {
    const result = registerSchema.safeParse({ ...valid, displayName: "" });
    expect(result.success).toBe(false);
  });

  it("rejects an invalid email address", () => {
    const result = registerSchema.safeParse({ ...valid, email: "not-an-email" });
    expect(result.success).toBe(false);
  });

  it("rejects a password shorter than 10 characters", () => {
    const result = registerSchema.safeParse({ ...valid, password: "Short1" });
    expect(result.success).toBe(false);
  });

  it("rejects a password with no uppercase letter", () => {
    const result = registerSchema.safeParse({ ...valid, password: "lowercase123" });
    expect(result.success).toBe(false);
  });

  it("rejects a password with no digit", () => {
    const result = registerSchema.safeParse({ ...valid, password: "NoDigitsHere" });
    expect(result.success).toBe(false);
  });

  it("accepts a password that is exactly 10 characters with an uppercase letter and a digit", () => {
    const result = registerSchema.safeParse({ ...valid, password: "Abcdefghi1" });
    expect(result.success).toBe(true);
  });
});

describe("loginSchema", () => {
  it("accepts a non-empty email and password", () => {
    expect(loginSchema.safeParse({ email: "alex@example.com", password: "anything" }).success).toBe(true);
  });

  it("rejects an empty password even though login has no strength requirement", () => {
    const result = loginSchema.safeParse({ email: "alex@example.com", password: "" });
    expect(result.success).toBe(false);
  });
});

describe("forgotPasswordSchema", () => {
  it("accepts a valid email", () => {
    expect(forgotPasswordSchema.safeParse({ email: "alex@example.com" }).success).toBe(true);
  });

  it("rejects an invalid email", () => {
    expect(forgotPasswordSchema.safeParse({ email: "not-an-email" }).success).toBe(false);
  });

  it("rejects an empty email", () => {
    expect(forgotPasswordSchema.safeParse({ email: "" }).success).toBe(false);
  });
});

describe("resetPasswordSchema", () => {
  it("accepts a password meeting the same strength rule as registration", () => {
    expect(resetPasswordSchema.safeParse({ newPassword: "GoodPass123" }).success).toBe(true);
  });

  it("rejects a password shorter than 10 characters", () => {
    expect(resetPasswordSchema.safeParse({ newPassword: "Short1" }).success).toBe(false);
  });

  it("rejects a password with no uppercase letter", () => {
    expect(resetPasswordSchema.safeParse({ newPassword: "lowercase123" }).success).toBe(false);
  });

  it("rejects a password with no digit", () => {
    expect(resetPasswordSchema.safeParse({ newPassword: "NoDigitsHere" }).success).toBe(false);
  });
});

describe("profileSchema", () => {
  it("makes bio optional", () => {
    const result = profileSchema.safeParse({ displayName: "Alex", timeZone: "America/New_York" });
    expect(result.success).toBe(true);
  });

  it("rejects a bio over 500 characters", () => {
    const result = profileSchema.safeParse({
      displayName: "Alex",
      timeZone: "America/New_York",
      bio: "a".repeat(501),
    });
    expect(result.success).toBe(false);
  });
});

describe("dreamStatementSchema", () => {
  const valid = {
    title: "Cut waste for small manufacturers",
    statement: "Help small manufacturers reduce waste through better tracking.",
    isBusinessShaped: true,
  };

  it("accepts the required fields with every optional field omitted", () => {
    expect(dreamStatementSchema.safeParse(valid).success).toBe(true);
  });

  it("rejects a title over 200 characters", () => {
    const result = dreamStatementSchema.safeParse({ ...valid, title: "a".repeat(201) });
    expect(result.success).toBe(false);
  });

  it("rejects a statement over 2000 characters", () => {
    const result = dreamStatementSchema.safeParse({ ...valid, statement: "a".repeat(2001) });
    expect(result.success).toBe(false);
  });

  it("requires isBusinessShaped to be a boolean", () => {
    // safeParse's input type is `unknown`, so this is a runtime check, not a compile-time one —
    // deliberately passing a non-boolean to prove the schema itself rejects it.
    const result = dreamStatementSchema.safeParse({ ...valid, isBusinessShaped: "yes" as unknown });
    expect(result.success).toBe(false);
  });
});

describe("createPostSchema", () => {
  it("accepts each valid visibility value", () => {
    for (const visibility of ["private", "community", "public"] as const) {
      expect(
        createPostSchema.safeParse({ body: "Hello", visibility, attachDream: false }).success,
      ).toBe(true);
    }
  });

  it("rejects a visibility value outside the enum", () => {
    const result = createPostSchema.safeParse({ body: "Hello", visibility: "everyone", attachDream: false });
    expect(result.success).toBe(false);
  });

  it("rejects an empty body", () => {
    const result = createPostSchema.safeParse({ body: "", visibility: "public", attachDream: false });
    expect(result.success).toBe(false);
  });
});

describe("reportContentSchema", () => {
  it("makes details optional", () => {
    expect(reportContentSchema.safeParse({ reason: "spam" }).success).toBe(true);
  });

  it("rejects a reason outside the enum", () => {
    const result = reportContentSchema.safeParse({ reason: "because I said so" });
    expect(result.success).toBe(false);
  });
});

describe("becomeMentorSchema", () => {
  it("requires at least one expertise entry", () => {
    const result = becomeMentorSchema.safeParse({ expertise: "" });
    expect(result.success).toBe(false);
  });

  it("accepts expertise with the optional fields omitted", () => {
    expect(becomeMentorSchema.safeParse({ expertise: "marketing, sales" }).success).toBe(true);
  });
});

describe("createHelpRequestSchema", () => {
  const valid = {
    category: "marketing", title: "Need help", body: "Where do I find customers?", attachDream: false,
  } as const;

  it("accepts every valid category", () => {
    for (const category of [
      "business", "marketing", "technology", "finance", "sales", "design", "career", "operations", "leadership",
    ] as const) {
      expect(createHelpRequestSchema.safeParse({ ...valid, category }).success).toBe(true);
    }
  });

  it("rejects a category outside the enum", () => {
    const result = createHelpRequestSchema.safeParse({ ...valid, category: "cooking" });
    expect(result.success).toBe(false);
  });
});
