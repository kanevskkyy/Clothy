import { z } from "zod";

export const sizeSchema = z.object({
    name: z.string()
        .min(1, "Size name is required.")
        .max(10, "Size name must be at most 10 characters."),
});

export type SizeSchema = z.infer<typeof sizeSchema>;