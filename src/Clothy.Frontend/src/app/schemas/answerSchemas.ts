import {z} from "zod";

export const answerSchemas = z.object({
    answerText: z.string()
        .min(2, "Answer text must be at least 2 characters")
        .max(500, "Answer text must not exceed 500 characters")
});

export type AnswerSchema = z.infer<typeof answerSchemas>;