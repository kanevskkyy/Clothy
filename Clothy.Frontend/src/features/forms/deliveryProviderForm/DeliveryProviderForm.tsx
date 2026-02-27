import React, { useRef, useState } from "react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import styles from "./DeliveryProviderForm.module.css";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import {
    deliveryProviderSchema,
    deliveryProviderUpdateSchema,
} from "../../../app/schemas/deliveryProviderSchema.ts";
import { ordersApi } from "../../../app/api/ordersApi.ts";

interface FormData {
    name: string;
    icon?: File;
}

interface Props {
    deliveryProviderId?: string;
    initialData?: { name?: string };
    method?: "create" | "update";
    onSuccess?: () => void;
}

const DeliveryProviderForm = ({
                                  deliveryProviderId,
                                  initialData,
                                  method = "create",
                                  onSuccess,
                              }: Props) => {
    const [formData, setFormData] = useState<FormData>({
        name: initialData?.name ?? "",
        icon: undefined,
    });

    const [errors, setErrors] = useState<Partial<Record<keyof FormData, string>>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const fileInputRef = useRef<HTMLInputElement>(null);

    const handleChange = (field: keyof FormData, value: string | File) => {
        setFormData(prev => ({ ...prev, [field]: value }));
        if (errors[field]) {
            setErrors(prev => ({ ...prev, [field]: undefined }));
        }
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const schema = method === "update" ? deliveryProviderUpdateSchema : deliveryProviderSchema;
        const result = schema.safeParse(formData);

        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        const formDataToSend = new FormData();
        formDataToSend.append("name", result.data.name);
        if (result.data.icon) {
            formDataToSend.append("icon", result.data.icon);
        }

        setIsSubmitting(true);

        try {
            if (method === "update" && deliveryProviderId) {
                await ordersApi.updateDeliveryProviderAsync(deliveryProviderId, formDataToSend);
                toast.success("Delivery provider updated successfully.");
            } else {
                await ordersApi.createDeliveryProviderAsync(formDataToSend);
                toast.success("Delivery provider created successfully.");
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
                    placeholder="enter provider name"
                    value={formData.name}
                    onChange={(e) => handleChange("name", e.target.value)}
                    error={!!errors.name}
                />
            </FormField>

            <FormField
                label="Icon"
                htmlFor="icon"
                required={method === "create"}
                error={errors.icon}
            >
                <input
                    ref={fileInputRef}
                    id="icon"
                    type="file"
                    accept=".jpg,.jpeg,.png,.gif,.svg"
                    className={styles.hiddenInput}
                    onChange={(e) => {
                        const file = e.target.files?.[0];
                        if (file) handleChange("icon", file);
                    }}
                />
                <div className={styles.fileRow}>
                    <Button
                        type="button"
                        variant="outline"
                        size="sm"
                        onClick={() => fileInputRef.current?.click()}
                    >
                        Choose file
                    </Button>
                    <span className={styles.fileName}>
                        {formData.icon ? formData.icon.name : "No file chosen"}
                    </span>
                </div>
            </FormField>

            <Button
                disabled={isSubmitting}
                type="submit"
                variant="primary"
                size="lg"
                fullWidth
            >
                {isSubmitting
                    ? (method === "update" ? "Saving..." : "Creating...")
                    : (method === "update" ? "Save changes" : "Create delivery provider")}
            </Button>
        </form>
    );
};

export default DeliveryProviderForm;