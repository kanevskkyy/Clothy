import { z } from "zod";

export const tagSchema = z.object({
    name: z.string().min(1, "Tag name is required").max(50, "Tag name must be at most 50 characters"),
    slug: z
        .string()
        .min(1, "Slug is required")
        .max(100, "Slug must be at most 100 characters")
        .regex(/^[a-z0-9]+(-[a-z0-9]+)*$/, "Slug can contain only letters, numbers, single dashes, cannot start/end with dash or have consecutive dashes")
        .transform((val) => val.toLowerCase()),
});

export type TagSchema = z.infer<typeof tagSchema>;