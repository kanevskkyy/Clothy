import React, { useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import styles from "./TagForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import {tagSchema, type TagSchema} from "../../../app/api/tagSchema.ts";

interface TagFormProps {
    tagId?: string;
    initialData?: Partial<TagSchema>;
    method?: "create" | "update";
    onSuccess?: () => void;
}

const TagForm = ({ tagId, initialData, method = "create", onSuccess }: TagFormProps) => {
    const [formData, setFormData] = useState<TagSchema>({
        name: initialData?.name ?? "",
        slug: initialData?.slug ?? "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof TagSchema, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleChange = (field: keyof TagSchema, value: string) => {
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

        const result = tagSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            if (method === "update" && tagId) {
                await catalogApi.updateTagAsync(tagId, result.data);
                toast.success("Tag updated successfully.");
            } else {
                await catalogApi.createTagAsync(result.data);
                toast.success("Tag created successfully.");
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
                    placeholder="Enter tag name"
                    value={formData.name}
                    onChange={(e) => handleNameChange(e.target.value)}
                    error={!!errors.name}
                />
            </FormField>

            <FormField label="Slug" htmlFor="slug" required error={errors.slug}>
                <Input
                    id="slug"
                    placeholder="Enter slug"
                    value={formData.slug}
                    onChange={(e) => handleChange("slug", e.target.value)}
                    error={!!errors.slug}
                />
            </FormField>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting
                    ? method === "update"
                        ? "Saving..."
                        : "Creating..."
                    : method === "update"
                        ? "Save changes"
                        : "Create tag"}
            </Button>
        </form>
    );
};

export default TagForm;