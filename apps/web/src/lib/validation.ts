import { z } from "zod";

// Zod's object schemas opportunistically JIT-compile a faster validator via `new Function(...)`,
// probing once at startup whether that's even possible. Under this app's CSP (script-src has no
// 'unsafe-eval' — see middleware.ts) the probe throws, and Zod catches it internally and falls
// back to its normal (always-correct, slightly slower) validation path with no functional impact.
// But Chrome's CSP enforcement reports the caught `new Function` as a securitypolicyviolation
// regardless of it being caught — this is a known upstream issue (zod#4461, zod#5414; see
// node_modules/zod/src/v4/core/util.ts's `allowsEval` and its own regression test,
// jitless-allows-eval.test.ts). `jitless: true` makes Zod skip the probe entirely instead of
// attempting-and-catching it, which is the maintainers' own sanctioned fix — not a CSP weakening,
// and not a validation-behavior change (this schema module is the single place every z.object()
// in the app is defined, and this call must run before any of them, per Zod's own contract).
z.config({ jitless: true });

export const registerSchema = z.object({
  displayName: z.string().min(1, "Enter your name").max(120, "Name is too long"),
  email: z.string().min(1, "Enter your email").email("Enter a valid email address"),
  password: z
    .string()
    .min(10, "Use at least 10 characters")
    .regex(/[A-Z]/, "Include at least one uppercase letter")
    .regex(/[0-9]/, "Include at least one number"),
});
export type RegisterInput = z.infer<typeof registerSchema>;

export const loginSchema = z.object({
  email: z.string().min(1, "Enter your email").email("Enter a valid email address"),
  password: z.string().min(1, "Enter your password"),
});
export type LoginInput = z.infer<typeof loginSchema>;

export const forgotPasswordSchema = z.object({
  email: z.string().min(1, "Enter your email").email("Enter a valid email address"),
});
export type ForgotPasswordInput = z.infer<typeof forgotPasswordSchema>;

/** Same request shape as forgotPasswordSchema, kept as a separate export so each page's form has
 * its own named type — makes call sites self-documenting about which flow they're in. */
export const resendVerificationSchema = forgotPasswordSchema;
export type ResendVerificationInput = z.infer<typeof resendVerificationSchema>;

export const resetPasswordSchema = z.object({
  newPassword: z
    .string()
    .min(10, "Use at least 10 characters")
    .regex(/[A-Z]/, "Include at least one uppercase letter")
    .regex(/[0-9]/, "Include at least one number"),
});
export type ResetPasswordInput = z.infer<typeof resetPasswordSchema>;

export const profileSchema = z.object({
  displayName: z.string().min(1, "Enter your name").max(120, "Name is too long"),
  bio: z.string().max(500, "Keep your bio under 500 characters").optional(),
  timeZone: z.string().min(1, "Select a time zone"),
});
export type ProfileInput = z.infer<typeof profileSchema>;

export const dreamStatementSchema = z.object({
  title: z.string().min(1, "Give your dream a short title").max(200, "Keep the title under 200 characters"),
  statement: z.string().min(1, "Write your dream statement").max(2000, "Keep this under 2000 characters"),
  purpose: z.string().max(2000, "Keep this under 2000 characters").optional(),
  whoItHelps: z.string().max(2000, "Keep this under 2000 characters").optional(),
  problem: z.string().max(2000, "Keep this under 2000 characters").optional(),
  outcome: z.string().max(2000, "Keep this under 2000 characters").optional(),
  motivation: z.string().max(2000, "Keep this under 2000 characters").optional(),
  impact: z.string().max(2000, "Keep this under 2000 characters").optional(),
  isBusinessShaped: z.boolean(),
});
export type DreamStatementInput = z.infer<typeof dreamStatementSchema>;

export const createActionSchema = z.object({
  title: z.string().min(1, "Give the action a title").max(200, "Keep the title under 200 characters"),
  description: z.string().max(2000, "Keep this under 2000 characters").optional(),
  priority: z.enum(["low", "medium", "high"]),
  difficulty: z.enum(["easy", "medium", "hard"]),
  expectedImpact: z.enum(["low", "medium", "high"]),
  /** Kept as a raw string from the number input — converted to a number at submit time, since
   * react-hook-form's register() returns string values and mixing zod coercion here confused
   * type inference for the resolver. */
  estimatedMinutes: z.string().optional(),
  dueDate: z.string().optional(),
});
export type CreateActionInput = z.infer<typeof createActionSchema>;

export const createExperimentSchema = z.object({
  ideaDescription: z.string().min(1, "Describe the idea you want to test").max(500, "Keep this under 500 characters"),
  hypothesis: z.string().min(1, "State what you expect to happen").max(1000, "Keep this under 1000 characters"),
  successCriteria: z.string().min(1, "Say how you'll know it worked").max(1000, "Keep this under 1000 characters"),
});
export type CreateExperimentInput = z.infer<typeof createExperimentSchema>;

export const recordExperimentResultSchema = z.object({
  outcome: z.enum(["validated", "partiallyValidated", "invalidated"]),
  evidence: z.string().max(2000, "Keep this under 2000 characters").optional(),
  learning: z.string().min(1, "Say what you learned").max(2000, "Keep this under 2000 characters"),
});
export type RecordExperimentResultInput = z.infer<typeof recordExperimentResultSchema>;

export const businessIdeaSchema = z.object({
  problem: z.string().max(5000, "Keep this under 5000 characters").optional(),
  customer: z.string().max(5000, "Keep this under 5000 characters").optional(),
  valueProposition: z.string().max(5000, "Keep this under 5000 characters").optional(),
  solution: z.string().max(5000, "Keep this under 5000 characters").optional(),
  businessModel: z.string().max(5000, "Keep this under 5000 characters").optional(),
  market: z.string().max(5000, "Keep this under 5000 characters").optional(),
  competitors: z.string().max(5000, "Keep this under 5000 characters").optional(),
  pricing: z.string().max(5000, "Keep this under 5000 characters").optional(),
  marketing: z.string().max(5000, "Keep this under 5000 characters").optional(),
  sales: z.string().max(5000, "Keep this under 5000 characters").optional(),
  operations: z.string().max(5000, "Keep this under 5000 characters").optional(),
  technology: z.string().max(5000, "Keep this under 5000 characters").optional(),
  financialAssumptions: z.string().max(5000, "Keep this under 5000 characters").optional(),
  risks: z.string().max(5000, "Keep this under 5000 characters").optional(),
});
export type BusinessIdeaInput = z.infer<typeof businessIdeaSchema>;

export const createPostSchema = z.object({
  body: z.string().min(1, "Say something").max(2000, "Keep this under 2000 characters"),
  visibility: z.enum(["private", "community", "public"]),
  // Opt-in, off by default - see post-composer.tsx and CreatePostCommand's doc comment on why
  // this is a plain boolean rather than a Dream id (the server resolves the signed-in user's own
  // Dream itself; the client never gets to say which Dream to attach).
  attachDream: z.boolean(),
});
export type CreatePostInput = z.infer<typeof createPostSchema>;

export const createCommentSchema = z.object({
  body: z.string().min(1, "Write a comment").max(1000, "Keep this under 1000 characters"),
});
export type CreateCommentInput = z.infer<typeof createCommentSchema>;

export const reportContentSchema = z.object({
  reason: z.enum(["spam", "harassment", "other"]),
  details: z.string().max(1000, "Keep this under 1000 characters").optional(),
});
export type ReportContentInput = z.infer<typeof reportContentSchema>;

export const becomeMentorSchema = z.object({
  expertise: z.string().min(1, "List at least one area of expertise"),
  yearsExperience: z.string().optional(),
  availability: z.string().max(50, "Keep this under 50 characters").optional(),
});
export type BecomeMentorInput = z.infer<typeof becomeMentorSchema>;

export const createHelpRequestSchema = z.object({
  category: z.enum([
    "business", "marketing", "technology", "finance", "sales", "design", "career", "operations", "leadership",
  ]),
  title: z.string().min(1, "Give this a title").max(200, "Keep the title under 200 characters"),
  body: z.string().min(1, "Describe what you need help with").max(2000, "Keep this under 2000 characters"),
  // Same opt-in pattern as createPostSchema.attachDream - see that field's comment.
  attachDream: z.boolean(),
});
export type CreateHelpRequestInput = z.infer<typeof createHelpRequestSchema>;

export const respondToHelpRequestSchema = z.object({
  body: z.string().min(1, "Write a response").max(2000, "Keep this under 2000 characters"),
});
export type RespondToHelpRequestInput = z.infer<typeof respondToHelpRequestSchema>;

export const notificationPreferencesSchema = z.object({
  emailProductUpdates: z.boolean(),
  emailCoachNudges: z.boolean(),
  emailCommunityActivity: z.boolean(),
});
export type NotificationPreferencesInput = z.infer<typeof notificationPreferencesSchema>;

/** Matches Waypoint.Users.Domain.VisibilityLevel exactly (camelCase, per the API's default JSON
 * enum naming policy - see Program.cs's JsonStringEnumConverter(JsonNamingPolicy.CamelCase)). */
export const visibilityLevelSchema = z.enum(["private", "followers", "community", "public"]);
export type VisibilityLevel = z.infer<typeof visibilityLevelSchema>;
export const privacySettingsSchema = z.object({
  profileVisibility: visibilityLevelSchema,
  dreamVisibility: visibilityLevelSchema,
});
export type PrivacySettingsInput = z.infer<typeof privacySettingsSchema>;
