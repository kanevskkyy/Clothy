import z from "zod";

export const updateReviewSchema = z.object({
    rating: z.number()
        .min(1, "Rating must be at least 1")
        .max(5, "Rating must be at most 5"),
    comment: z
        .string()
        .min(1, "Comment is required")
        .max(500, "Comment must not exceed 500 characters"),
});

export type UpdateReviewFormData = z.infer<typeof updateReviewSchema>;