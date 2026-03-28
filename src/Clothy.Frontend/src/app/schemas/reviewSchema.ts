import { z } from "zod";

export const reviewSchema = z.object({
    clotheItemId: z.string()
        .uuid("Invalid clothe item id"),

    rating: z.coerce.number()
        .int("Rating must be integer")
        .min(1, "Min value of rating is 1")
        .max(5, "Max value of rating is 5"),

    comment: z.string()
        .trim()
        .min(1, "Comment is required")
        .max(500, "Max length of comment cannot exceed 500 symbols"),
});

export type ReviewSchemaData = z.infer<typeof reviewSchema>;