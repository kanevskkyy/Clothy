import { z } from "zod";

export const colorSchema = z.object({
    name: z.string()
        .min(1, "Color name is required.")
        .max(20, "Color name must be at most 20 characters."),
    slug: z.string()
        .min(1, "Slug is required.")
        .max(40, "Slug cannot exceed 40 characters.")
        .toLowerCase()
        .regex(/^[a-z0-9]+(-[a-z0-9]+)*$/, "Slug can contain only letters, numbers, and single dashes, cannot start or end with a dash, or contain consecutive dashes."),
    hexCode: z.string()
        .min(1, "Hex code is required.")
        .max(7, "Hex code must be at most 7 characters.")
        .regex(/^#[0-9A-Fa-f]{6}$/, "Hex code must be a valid color code (e.g. #A1B2C3)."),
});

export type ColorSchema = z.infer<typeof colorSchema>;