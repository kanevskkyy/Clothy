import { z } from "zod";

export const collectionSchema = z.object({
    name: z.string()
        .min(1, "Name is required.")
        .max(100, "Name must be at most 100 characters."),

    slug: z.string()
        .min(1, "Slug is required.")
        .max(60, "Slug must be at most 60 characters.")
        .toLowerCase()
        .regex(
            /^[a-z0-9]+(-[a-z0-9]+)*$/,
            "Slug can contain only letters, numbers, and single dashes, cannot start or end with a dash, or contain consecutive dashes."
        ),
});

export type CollectionSchema = z.infer<typeof collectionSchema>;