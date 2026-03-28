import {z} from "zod";

export const registerSchema = z.object({
    email: z.string()
        .email("Email is required")
        .min(1, "Email is required")
        .max(100, "Email length must be less than 100 symbols"),
    password: z.string()
        .min(8, "The password is mandatory and must be at least 8 characters long")
        .regex(/[a-z]/,"The password must contain at least 1 lowercase letter")
        .regex(/[A-Z]/,"The password must contain at least 1 uppercase letter")
        .regex(/[0-9]/,"The password must contain at least 1 digit")
        .regex(/[^a-zA-Z0-9]/,"The password must contain at least 1 special character"),
    firstName: z.string()
        .min(1, "First name is required")
        .max(100, "First name must be less than 100 symbols"),
    lastName: z.string()
        .min(1, "Last name is required")
        .max(100, "Last name must be less than 100 symbols"),
    phoneNumber: z.string()
        .min(1, "Phone number is required")
        .regex(/^\+380\d{9}$/, "Phone must be in format +380XXXXXXXXX")
})

export type RegisterFormData = z.infer<typeof registerSchema>;