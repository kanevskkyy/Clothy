import { z } from "zod";

const permittedExtensions = [".jpg", ".jpeg", ".png", ".gif", ".svg"];
const maxFileSize = 5 * 1024 * 1024;

const iconSchema = z
    .instanceof(File, { message: "Icon is required." })
    .refine(
        (file) => {
            const ext = "." + file.name.split(".").pop()?.toLowerCase();
            return permittedExtensions.includes(ext ?? "");
        },
        { message: `File must be one of: ${permittedExtensions.join(", ")}` }
    )
    .refine(
        (file) => file.size > 0 && file.size <= maxFileSize,
        { message: "File must be smaller than 5 MB." }
    );

export const deliveryProviderSchema = z.object({
    name: z.string()
        .min(1, "Name is required.")
        .max(100, "Name cannot exceed 100 characters."),
    icon: iconSchema,
});

export const deliveryProviderUpdateSchema = z.object({
    name: z.string()
        .min(1, "Name is required.")
        .max(100, "Name cannot exceed 100 characters."),
    icon: iconSchema.optional(),
});

export type DeliveryProviderSchema = z.infer<typeof deliveryProviderSchema>;
export type DeliveryProviderUpdateSchema = z.infer<typeof deliveryProviderUpdateSchema>;