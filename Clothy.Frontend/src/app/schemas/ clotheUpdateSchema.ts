import { z } from "zod";

const genderValues = ["Male", "Female", "Unisex"] as const;

export const clotheUpdateSchema = z.object({
    name: z
        .string()
        .min(1, "Name is required.")
        .max(100, "Name cannot exceed 100 characters."),

    slug: z
        .string()
        .min(1, "Slug is required.")
        .max(60, "Slug cannot exceed 60 characters.")
        .refine((s) => s === s.toLowerCase(), {
            message: "Slug must be lowercase.",
        })
        .regex(
            /^[a-z0-9]+(-[a-z0-9]+)*$/,
            "Slug can only contain lowercase letters, numbers, and single dashes."
        ),

    description: z
        .string()
        .min(1, "Description is required.")
        .max(1000, "Description cannot exceed 1000 characters."),

    price: z
        .coerce.number({ message: "Price must be a number." })
        .gt(0, "Price must be greater than 0."),

    gender: z.enum(genderValues, {
        message: "Gender is required.",
    }),

    brandId: z.string().min(1, "Brand is required."),
    clothingTypeId: z.string().min(1, "Clothing type is required."),
    collectionId: z.string().min(1, "Collection is required."),
});

export type ClotheUpdateSchema = z.infer<typeof clotheUpdateSchema>;