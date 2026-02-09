import {z} from "zod";

export const forgotPasswordSchema = z.object({
    email: z.string()
        .min(1, "Email is required")
        .email("Invalid email address")
        .max(100, "Email length must be less than 100 symbols"),
});

export type ForgotPasswordFormData = z.infer<typeof forgotPasswordSchema>;