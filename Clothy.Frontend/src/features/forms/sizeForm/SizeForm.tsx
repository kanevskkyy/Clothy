import React, { useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import styles from "./SizeForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { sizeSchema, type SizeSchema } from "../../../app/schemas/sizeSchema.ts";
import { catalogApi } from "../../../app/api/catalogApi.ts";

interface SizeFormProps {
    sizeId?: string;
    initialData?: Partial<SizeSchema>;
    method?: "create" | "update";
    onSuccess?: () => void;
}

const SizeForm = ({
                      sizeId,
                      initialData,
                      method = "create",
                      onSuccess,
                  }: SizeFormProps) => {
    const [formData, setFormData] = useState<SizeSchema>({
        name: initialData?.name ?? "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof SizeSchema, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleChange = (field: keyof SizeSchema, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        if (errors[field]) setErrors(prev => ({ ...prev, [field]: undefined }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = sizeSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            if (method === "update" && sizeId) {
                await catalogApi.updateSizeAsync(sizeId, result.data);
                toast.success("Size updated successfully.");
            } else {
                await catalogApi.createSizeAsync(result.data);
                toast.success("Size created successfully.");
            }
            onSuccess?.();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>
            <FormField label="Size Name" htmlFor="name" required error={errors.name}>
                <Input
                    id="name"
                    placeholder="Enter size name (e.g. S, M, L)"
                    value={formData.name}
                    onChange={(e) => handleChange("name", e.target.value)}
                    error={!!errors.name}
                />
            </FormField>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting
                    ? (method === "update" ? "Saving..." : "Creating...")
                    : (method === "update" ? "Save changes" : "Create size")}
            </Button>
        </form>
    );
};

export default SizeForm;