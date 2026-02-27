import React, { useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import styles from "./CollectionForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { collectionSchema, type CollectionSchema } from "../../../app/schemas/collectionSchema.ts";
import { catalogApi } from "../../../app/api/catalogApi.ts";

interface CollectionFormProps {
    collectionId?: string;
    initialData?: Partial<CollectionSchema>;
    method?: "create" | "update";
    onSuccess?: () => void;
}

const CollectionForm = ({
                            collectionId,
                            initialData,
                            method = "create",
                            onSuccess,
                        }: CollectionFormProps) => {
    const [formData, setFormData] = useState<CollectionSchema>({
        name: initialData?.name ?? "",
        slug: initialData?.slug ?? "",
    });

    const [errors, setErrors] = useState<Partial<Record<keyof CollectionSchema, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);

    const handleChange = (field: keyof CollectionSchema, value: string) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        if (errors[field]) setErrors(prev => ({ ...prev, [field]: undefined }));
    };

    const handleNameChange = (value: string) => {
        handleChange("name", value);

        if (method === "create") {
            const autoSlug = value
                .toLowerCase()
                .trim()
                .replace(/\s+/g, "-")
                .replace(/[^a-z0-9-]/g, "");

            handleChange("slug", autoSlug);
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = collectionSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            if (method === "update" && collectionId) {
                await catalogApi.updateCollectionAsync(collectionId, result.data);
                toast.success("Collection updated successfully.");
            } else {
                await catalogApi.createCollectionAsync(result.data);
                toast.success("Collection created successfully.");
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
                    placeholder="enter collection name"
                    value={formData.name}
                    onChange={(e) => handleNameChange(e.target.value)}
                    error={!!errors.name}
                />
            </FormField>

            <FormField label="Slug" htmlFor="slug" required error={errors.slug}>
                <Input
                    id="slug"
                    placeholder="enter collection slug"
                    value={formData.slug}
                    onChange={(e) => handleChange("slug", e.target.value)}
                    error={!!errors.slug}
                />
            </FormField>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting
                    ? (method === "update" ? "Saving..." : "Creating...")
                    : (method === "update" ? "Save changes" : "Create collection")}
            </Button>
        </form>
    );
};

export default CollectionForm;