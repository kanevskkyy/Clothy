import {z} from "zod";

export const resetPasswordSchema = z.object({
    currentPassword: z.string()
        .min(1, "Current password is required"),

    newPassword: z.string()
        .min(8, "The password is mandatory and must be at least 8 characters long")
        .regex(/[a-z]/,"The password must contain at least 1 lowercase letter")
        .regex(/[A-Z]/,"The password must contain at least 1 uppercase letter")
        .regex(/[0-9]/,"The password must contain at least 1 digit")
        .regex(/[^a-zA-Z0-9]/,"The password must contain at least 1 special character"),
})

export type ResetPasswordSchema = z.infer<typeof resetPasswordSchema>;