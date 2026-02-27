import { z } from "zod";

const permittedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".svg", ".webp"];
const maxFileSize = 5 * 1024 * 1024;

export const clothePhotoSchema = z.object({
    photo: z
        .instanceof(File, { message: "Photo is required." })
        .refine(
            (file) => permittedExtensions.includes("." + file.name.split(".").pop()?.toLowerCase()),
            { message: `Photo must be one of: ${permittedExtensions.join(", ")}` }
        )
        .refine(
            (file) => file.size > 0 && file.size <= maxFileSize,
            { message: "Photo must be smaller than 5 MB." }
        ),
    colorId: z.string().min(1, "Color is required."),
    isMain: z.boolean(),
});

export const clotheMaterialSchema = z.object({
    materialId: z.string().min(1, "Material is required."),
    percentage: z
        .number({ message: "Must be a number." })
        .min(0, "Min 0.")
        .max(100, "Max 100."),
});

export const clotheCreateSchema = z.object({
    name: z.string().min(1, "Name is required.").max(100),
    slug: z
        .string()
        .min(1, "Slug is required.")
        .max(60, "Slug cannot exceed 60 characters.")
        .refine((s) => s === s.toLowerCase(), "Slug must be lowercase.")
        .regex(
            /^[a-z][a-z0-9]*(-[a-z0-9]+)*$/,  
            "Slug must start with a letter and contain only lowercase letters, numbers, and single dashes."
        ),
    description: z.string().min(1, "Description is required.").max(1000),
    price: z.number({ message: "Price must be a number." }).gt(0, "Price must be greater than 0."),
    gender: z
        .enum(["Male", "Female", "Unisex"])
        .refine((val) => val !== undefined, {
            message: "Gender is required.",
        }),
    brandId: z.string().min(1, "Brand is required."),
    clothingTypeId: z.string().min(1, "Clothing type is required."),
    collectionId: z.string().min(1, "Collection is required."),
    tagIds: z.array(z.string()).min(1, "At least one tag must be selected."),
    additionalPhotos: z.array(clothePhotoSchema).min(1, "At least one photo is required."),
    materials: z
        .array(clotheMaterialSchema)
        .refine(
            (materials) => materials.length === 0 || materials.reduce((sum, m) => sum + m.percentage, 0) === 100,
            { message: "Total material percentage must equal 100%." }
        ),
});

export type ClotheCreateSchema = z.infer<typeof clotheCreateSchema>;