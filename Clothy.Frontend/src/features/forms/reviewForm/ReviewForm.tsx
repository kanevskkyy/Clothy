import React, {useState} from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Textarea from "../../../shared/ui/Textarea/Textarea.tsx";
import styles from "./ReviewForm.module.css";
import {toast} from "sonner";
import {getErrorMessage} from "../../../shared/lib/errorHandler.ts";
import {getZodFieldErrors} from "../../../shared/lib/getZodFieldErrors.ts";
import {reviewSchema, type ReviewSchemaData} from "../../../app/schemas/reviewSchema.ts";
import {reviewApi} from "../../../app/api/reviewApi.ts";

interface ReviewCreateFormProps {
    clotheId?: string;
    reviewId?: string;
    initialData?: Partial<ReviewSchemaData>;
    method?: "create" | "update";
    onSuccess?: () => void;
}

const ReviewForm = ({
                              clotheId,
                              reviewId,
                              initialData,
                              method = "create",
                              onSuccess,
                          }: ReviewCreateFormProps) => {
    const [formData, setFormData] = useState<ReviewSchemaData>({
        clotheItemId: clotheId ?? "",
        rating: initialData?.rating ?? 0,
        comment: initialData?.comment ?? "",
    });
    const [hoveredRating, setHoveredRating] = useState(0);
    const [errors, setErrors] = useState<Partial<Record<keyof ReviewSchemaData, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = reviewSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            if (method === "update" && reviewId) {
                await reviewApi.updateReviewAsync(reviewId, result.data);
                toast.success("Your review has been updated and will appear once approved by moderators.");
            } else {
                await reviewApi.createReviewAsync(result.data);
                toast.success("Your review has been submitted and will appear once approved by moderators.");
            }
            if (onSuccess) onSuccess();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <FormField label="Rating: " htmlFor="rating" required flexDirection="row" error={errors.rating}>
                <div className={styles.stars}>
                    {Array.from({length: 5}, (_, index) => {
                        const value = index + 1;
                        return (
                            <span
                                key={index}
                                className={`${styles.star} ${value <= (hoveredRating || formData.rating) ? styles.filled : ""}`}
                                onClick={() => {
                                    setFormData(prev => ({...prev, rating: value}));
                                    if (errors.rating) setErrors(prev => ({...prev, rating: undefined}));
                                }}
                                onMouseEnter={() => setHoveredRating(value)}
                                onMouseLeave={() => setHoveredRating(0)}
                            >
                                ★
                            </span>
                        );
                    })}
                </div>
            </FormField>

            <FormField label="Comment: " htmlFor="comment" required error={errors.comment}>
                <Textarea
                    id="comment"
                    placeholder="share your thoughts about this product..."
                    value={formData.comment}
                    onChange={(e) => {
                        setFormData(prev => ({...prev, comment: e.target.value}));
                        if (errors.comment) setErrors(prev => ({...prev, comment: undefined}));
                    }}
                    error={!!errors.comment}
                />
            </FormField>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting
                    ? (method === "update" ? "Saving..." : "Submitting...")
                    : (method === "update" ? "Save changes" : "Submit review")}
            </Button>
        </form>
    );
};

export default ReviewForm;