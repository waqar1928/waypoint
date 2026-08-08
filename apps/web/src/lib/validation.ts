import { z } from "zod";

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
