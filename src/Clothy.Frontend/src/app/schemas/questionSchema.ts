import {z} from "zod";

export const questionSchema = z.object({
    clotheItemId: z.string()
        .uuid("Invalid clothe item id"),

    questionText: z.string()
        .min(5, "Question text must be at least 5 characters")
        .max(500, "Question text must not exceed 500 characters")
});

export type QuestionSchema = z.infer<typeof questionSchema>;