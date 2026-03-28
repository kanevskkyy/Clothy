import React, { useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import styles from "./ColorForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { colorSchema, type ColorSchema } from "../../../app/schemas/colorSchema.ts";
import { catalogApi } from "../../../app/api/catalogApi.ts";

interface ColorFormProps {
    colorId?: string;
    initialData?: Partial<ColorSchema>;
    method?: "create" | "update";
    onSuccess?: () => void;
}

const ColorForm = ({ colorId, initialData, method = "create", onSuccess }: ColorFormProps) => {
    const [formData, setFormData] = useState<ColorSchema>({
        name: initialData?.name ?? "",
        slug: initialData?.slug ?? "",
        hexCode: initialData?.hexCode ?? "#000000",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof ColorSchema, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleChange = (field: keyof ColorSchema, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        if (errors[field]) setErrors(prev => ({ ...prev, [field]: undefined }));
    };

    const handleNameChange = (value: string) => {
        handleChange("name", value);
        if (method === "create") {
            const autoSlug = value.toLowerCase().trim().replace(/\s+/g, "-").replace(/[^a-z0-9-]/g, "");
            handleChange("slug", autoSlug);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = colorSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            if (method === "update" && colorId) {
                await catalogApi.updateColorAsync(colorId, result.data);
                toast.success("Color updated successfully.");
            } else {
                await catalogApi.createColorAsync(result.data);
                toast.success("Color created successfully.");
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
            <FormField label="Name" htmlFor="name" required error={errors.name}>
                <Input
                    id="name"
                    placeholder="enter color name"
                    value={formData.name}
                    onChange={(e) => handleNameChange(e.target.value)}
                    error={!!errors.name}
                />
            </FormField>

            <FormField label="Slug" htmlFor="slug" required error={errors.slug}>
                <Input
                    id="slug"
                    placeholder="enter slug"
                    value={formData.slug}
                    onChange={(e) => handleChange("slug", e.target.value)}
                    error={!!errors.slug}
                />
            </FormField>

            <FormField label="Color" htmlFor="hexCode" required error={errors.hexCode}>
                <div className={styles.colorInputWrapper}>
                    <Input
                        type="color"
                        id="hexCode"
                        value={formData.hexCode}
                        onChange={(e) => handleChange("hexCode", e.target.value)}
                        className={styles.colorPicker}
                    />
                </div>
            </FormField>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting
                    ? (method === "update" ? "Saving..." : "Creating...")
                    : (method === "update" ? "Save changes" : "Create color")}
            </Button>
        </form>
    );
};

export default ColorForm;