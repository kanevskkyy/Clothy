import { z } from "zod";

const PERMITTED_EXTENSIONS = [".jpg", ".jpeg", ".png", ".gif", ".svg"];
const MAX_FILE_SIZE = 5 * 1024 * 1024;

export const userUpdateSchema = z.object({
    firstName: z
        .string()
        .min(1, "First name is required.")
        .max(100, "First name cannot exceed 100 characters."),
    lastName: z
        .string()
        .min(1, "Last name is required.")
        .max(100, "Last name cannot exceed 100 characters."),
    phoneNumber: z
        .string()
        .min(1, "Phone number is required.")
        .regex(/^\+380\d{9}$/, "Phone number must be in Ukrainian format: +380XXXXXXXXX."),
    photo: z
        .instanceof(File)
        .optional()
        .refine(
            (file) => {
                if (!file) return true;
                const ext = file.name.substring(file.name.lastIndexOf(".")).toLowerCase();
                return PERMITTED_EXTENSIONS.includes(ext);
            },
            "Invalid file type."
        )
        .refine(
            (file) => {
                if (!file) return true;
                return file.size > 0 && file.size <= MAX_FILE_SIZE;
            },
            "File must be smaller than 5 MB."
        ),
});

export type UserUpdateFormData = z.infer<typeof userUpdateSchema>;