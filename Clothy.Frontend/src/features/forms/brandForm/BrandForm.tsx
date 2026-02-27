import React, { useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import styles from "./BrandForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { brandSchema, type BrandSchema } from "../../../app/schemas/brandSchema.ts";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import { useQueryClient } from "@tanstack/react-query";

interface BrandFormProps {
    brandId?: string;
    initialData?: Partial<BrandSchema>;
    method?: "create" | "update";
    onSuccess?: () => void;
}

const BrandForm = ({
                       brandId,
                       initialData,
                       method = "create",
                       onSuccess,
                   }: BrandFormProps) => {
    const [formData, setFormData] = useState<BrandSchema>({
        name: initialData?.name ?? "",
        slug: initialData?.slug ?? "",
    });
    const [errors, setErrors] = useState<Partial<Record<keyof BrandSchema, string>>>({});

    const [isSubmitting, setIsSubmitting] = useState(false);
    const queryClient = useQueryClient();

    const handleChange = (field: keyof BrandSchema, value: string) => {
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

        const result = brandSchema.safeParse(formData);
        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            if (method === "update" && brandId) {
                await catalogApi.updateBrandAsync(brandId, result.data);
                toast.success("Brand updated successfully.");
            } else {
                await catalogApi.createBrandAsync(result.data);
                toast.success("Brand created successfully.");
            }

            await queryClient.invalidateQueries({ queryKey: ["brands"] });
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
                    placeholder="enter brand name"
                    value={formData.name}
                    onChange={(e) => handleNameChange(e.target.value)}
                    error={!!errors.name}
                />
            </FormField>

            <FormField label="Slug" htmlFor="slug" required error={errors.slug}>
                <Input
                    id="slug"
                    placeholder="enter brand slug"
                    value={formData.slug}
                    onChange={(e) => handleChange("slug", e.target.value)}
                    error={!!errors.slug}
                />
            </FormField>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting
                    ? (method === "update" ? "Saving..." : "Creating...")
                    : (method === "update" ? "Save changes" : "Create brand")}
            </Button>
        </form>
    );
};

export default BrandForm;