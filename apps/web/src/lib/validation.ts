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
