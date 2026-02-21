import React, { useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Textarea from "../../../shared/ui/Textarea/Textarea.tsx";
import styles from "./AnswerCreateForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { type AnswerSchema, answerSchemas } from "../../../app/schemas/answerSchemas.ts";
import { questionApi } from "../../../app/api/questionApi.ts";
import type { IAnswerReadDTO } from "../../../entities/reviewsService/answers/IAnswerReadDTO.ts";

interface AnswerCreateFormProps {
    questionId: string;
    onSuccess?: (newAnswer: IAnswerReadDTO) => void;
    onCancel?: () => void;
}

const AnswerCreateForm = ({ questionId, onSuccess, onCancel }: AnswerCreateFormProps) => {
    const [formData, setFormData] = useState<AnswerSchema>({ answerText: "" });
    const [errors, setErrors] = useState<Partial<Record<keyof AnswerSchema, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = answerSchemas.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            const newAnswer = await questionApi.createAnswerAsync(questionId, result.data);
            toast.success("Your answer has been submitted successfully");
            setFormData({ answerText: "" });
            onSuccess?.(newAnswer);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <FormField label="Your answer:" htmlFor="answer" required error={errors.answerText}>
                <Textarea
                    id="answer"
                    placeholder="write your answer here..."
                    value={formData.answerText}
                    onChange={(e) => {
                        setFormData(prev => ({ ...prev, answerText: e.target.value }));
                        if (errors.answerText) setErrors(prev => ({ ...prev, answerText: undefined }));
                    }}
                    error={!!errors.answerText}
                />
            </FormField>

            <div className={styles.actions}>
                <Button type="button" variant="outline" size="sm" onClick={onCancel}>
                    Cancel
                </Button>
                <Button type="submit" variant="primary" size="sm" disabled={isSubmitting}>
                    {isSubmitting ? "Submitting..." : "Submit answer"}
                </Button>
            </div>
        </form>
    );
};

export default AnswerCreateForm;