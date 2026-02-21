import React, { useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Textarea from "../../../shared/ui/Textarea/Textarea.tsx";
import styles from "./QuestionCreateForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { type QuestionSchema, questionSchema } from "../../../app/schemas/questionSchema.ts";
import { questionApi } from "../../../app/api/questionApi.ts";
import type { IQuestionReadDTO } from "../../../entities/reviewsService/questions/IQuestionReadDTO.ts";

interface QuestionCreateFormProps {
    clotheId: string;
    onSuccess?: (newQuestion: IQuestionReadDTO) => void;
}

const QuestionCreateForm = ({ clotheId, onSuccess }: QuestionCreateFormProps) => {
    const [formData, setFormData] = useState<QuestionSchema>({
        clotheItemId: clotheId,
        questionText: "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof QuestionSchema, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = questionSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            const newQuestion = await questionApi.createQuestionAsync(result.data);
            toast.success("Your question has been submitted successfully");
            setFormData({ clotheItemId: clotheId, questionText: "" });
            onSuccess?.(newQuestion);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <FormField label="Your question: " htmlFor="question" required error={errors.questionText}>
                <Textarea
                    id="question"
                    placeholder="type your question here..."
                    value={formData.questionText}
                    onChange={(e) => {
                        setFormData(prev => ({ ...prev, questionText: e.target.value }));
                        if (errors.questionText) setErrors(prev => ({ ...prev, questionText: undefined }));
                    }}
                    error={!!errors.questionText}
                />
            </FormField>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting ? "Submitting..." : "Submit question"}
            </Button>
        </form>
    );
};

export default QuestionCreateForm;