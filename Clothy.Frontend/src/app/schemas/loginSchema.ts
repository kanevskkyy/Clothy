import {z} from "zod";

export const loginSchema = z.object({
    email: z.string()
        .min(1, "Email is required")
        .email("Invalid email address")
        .max(100, "Email length must be less than 100 symbols"),
    password: z.string()
        .min(1, "Password is required")
});

export type LoginFormData = z.infer<typeof loginSchema>;