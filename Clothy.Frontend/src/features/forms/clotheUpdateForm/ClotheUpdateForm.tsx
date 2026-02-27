import React, { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { toast } from "sonner";
import { Trash2 } from "lucide-react";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import Textarea from "../../../shared/ui/Textarea/Textarea.tsx";
import Select from "../../../shared/ui/Select/Select.tsx";
import RadioOption from "../../../shared/ui/RadioOption/RadioOption.tsx";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import type { IClotheByIdDTO, IClotheStockDTO } from "../../../entities/catalogService/interfaces/clothe/IClotheDetailDTO.ts";
import styles from "./ClotheUpdateForm.module.css";
import {clotheUpdateSchema} from "../../../app/schemas/ clotheUpdateSchema.ts";

type FormData = {
    name: string;
    slug: string;
    description: string;
    price: number | "";
    gender: "Male" | "Female" | "Unisex" | "";
    brandId: string;
    clothingTypeId: string;
    collectionId: string;
};

type StockDraft = {
    stockId: string;
    quantity: number;
    sizeName: string;
    colorName: string;
    hexCode: string;
    saving: boolean;
};

interface Props {
    clothe: IClotheByIdDTO;
    onSuccess?: () => void;
}

const ClotheUpdateForm = ({ clothe, onSuccess }: Props) => {
    const [formData, setFormData] = useState<FormData>({
        name: clothe.name,
        slug: clothe.slug,
        description: clothe.description,
        price: clothe.price,
        gender: clothe.gender as "Male" | "Female" | "Unisex",
        brandId: clothe.brand.id,
        clothingTypeId: clothe.clothyType.id,
        collectionId: clothe.collection.id,
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const [isDeleting, setIsDeleting] = useState(false);

    const [stocks, setStocks] = useState<StockDraft[]>(
        clothe.stocks.map((s: IClotheStockDTO) => ({
            stockId: s.stockId,
            quantity: s.quantity,
            sizeName: s.size.name,
            colorName: s.color.name,
            hexCode: s.color.hexCode,
            saving: false,
        }))
    );

    const { data: filters } = useQuery({
        queryKey: ["catalog-filters"],
        queryFn: () => catalogApi.getFiltersAsync(),
        staleTime: 1000 * 60 * 15,
    });

    const brandOptions = filters?.brands.map((b) => ({ value: b.id, label: b.name })) ?? [];
    const clothingTypeOptions = filters?.clothingTypes.map((c) => ({ value: c.id, label: c.name })) ?? [];
    const collectionOptions = filters?.collections.map((c) => ({ value: c.id, label: c.name })) ?? [];

    const clearError = (key: string) => {
        setErrors((prev) => { const next = { ...prev }; delete next[key]; return next; });
    };

    const handleField = (field: keyof FormData, value: unknown) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
        clearError(field);
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const result = clotheUpdateSchema.safeParse({
            ...formData,
            price: formData.price === "" ? undefined : Number(formData.price),
        });

        if (!result.success) {
            setErrors(getZodFieldErrors(result.error));
            return;
        }

        setIsSubmitting(true);
        try {
            await catalogApi.updateClotheAsync(clothe.id, result.data);
            toast.success("Clothe updated successfully.");
            onSuccess?.();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    };

    const handleDelete = () => {
        toast(`Delete "${clothe.name}"?`, {
            action: {
                label: "Delete",
                onClick: async () => {
                    try {
                        setIsDeleting(true);
                        await catalogApi.deleteClotheAsync(clothe.id);
                        toast.success("Clothe deleted.");
                        onSuccess?.();
                    } catch (error) {
                        toast.error(getErrorMessage(error));
                    } finally {
                        setIsDeleting(false);
                    }
                },
            },
        });
    };

    const handleStockSave = async (draft: StockDraft) => {
        setStocks((prev) =>
            prev.map((s) => (s.stockId === draft.stockId ? { ...s, saving: true } : s))
        );
        try {
            await catalogApi.updateStockAsync(draft.stockId, draft.quantity);
            toast.success("Stock updated.");
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setStocks((prev) =>
                prev.map((s) => (s.stockId === draft.stockId ? { ...s, saving: false } : s))
            );
        }
    };

    return (
        <div className={styles.wrapper}>
            <form className={styles.form} onSubmit={handleSubmit}>
                <section className={styles.section}>
                    <h3 className={styles.sectionTitle}>Basic Info</h3>
                    <div className={styles.row2}>
                        <FormField label="Name" htmlFor="name" required error={errors.name}>
                            <Input
                                id="name"
                                value={formData.name}
                                onChange={(e) => handleField("name", e.target.value)}
                                error={!!errors.name}
                            />
                        </FormField>
                        <FormField label="Slug" htmlFor="slug" required error={errors.slug}>
                            <Input
                                id="slug"
                                value={formData.slug}
                                onChange={(e) => handleField("slug", e.target.value)}
                                error={!!errors.slug}
                            />
                        </FormField>
                    </div>

                    <FormField label="Description" htmlFor="description" required error={errors.description}>
                        <Textarea
                            id="description"
                            rows={4}
                            value={formData.description}
                            onChange={(e) => handleField("description", e.target.value)}
                            error={!!errors.description}
                        />
                    </FormField>

                    <FormField label="Price" htmlFor="price" required error={errors.price}>
                        <Input
                            id="price"
                            type="number"
                            min={0}
                            step="0.01"
                            value={formData.price}
                            onChange={(e) =>
                                handleField("price", e.target.value === "" ? "" : parseFloat(e.target.value))
                            }
                            error={!!errors.price}
                        />
                    </FormField>

                    <FormField label="Gender" htmlFor="gender" required error={errors.gender}>
                        <div className={styles.radioGroup}>
                            {(["Male", "Female", "Unisex"] as const).map((g) => (
                                <RadioOption
                                    key={g}
                                    id={`gender-${g}`}
                                    name="gender"
                                    value={g}
                                    label={g}
                                    checked={formData.gender === g}
                                    onChange={(v) => handleField("gender", v)}
                                />
                            ))}
                        </div>
                    </FormField>
                </section>

                <section className={styles.section}>
                    <h3 className={styles.sectionTitle}>Classification</h3>
                    <div className={styles.row3}>
                        <FormField label="Brand" htmlFor="brandId" required error={errors.brandId}>
                            <Select
                                inputId="brandId"
                                placeholder="Select brand"
                                options={brandOptions}
                                value={brandOptions.find((o) => o.value === formData.brandId) || null}
                                onChange={(opt: any) => handleField("brandId", opt?.value ?? "")}
                                error={!!errors.brandId}
                                menuPortalTarget={document.body}
                                menuPosition="fixed"
                            />
                        </FormField>
                        <FormField label="Clothing Type" htmlFor="clothingTypeId" required error={errors.clothingTypeId}>
                            <Select
                                inputId="clothingTypeId"
                                placeholder="Select type"
                                options={clothingTypeOptions}
                                value={clothingTypeOptions.find((o) => o.value === formData.clothingTypeId) || null}
                                onChange={(opt: any) => handleField("clothingTypeId", opt?.value ?? "")}
                                error={!!errors.clothingTypeId}
                                menuPortalTarget={document.body}
                                menuPosition="fixed"
                            />
                        </FormField>
                        <FormField label="Collection" htmlFor="collectionId" required error={errors.collectionId}>
                            <Select
                                inputId="collectionId"
                                placeholder="Select collection"
                                options={collectionOptions}
                                value={collectionOptions.find((o) => o.value === formData.collectionId) || null}
                                onChange={(opt: any) => handleField("collectionId", opt?.value ?? "")}
                                error={!!errors.collectionId}
                                menuPortalTarget={document.body}
                                menuPosition="fixed"
                            />
                        </FormField>
                    </div>
                </section>

                <div className={styles.formActions}>
                    <Button
                        disabled={isSubmitting || isDeleting}
                        type="submit"
                        variant="primary"
                        size="lg"
                        fullWidth
                    >
                        {isSubmitting ? "Saving..." : "Save changes"}
                    </Button>
                    <Button
                        variant="secondary"
                        icon={<Trash2 size={15} />}
                        disabled={isDeleting || isSubmitting}
                        onClick={handleDelete}
                    >
                        {isDeleting ? "Deleting..." : "Delete clothe"}
                    </Button>
                </div>
            </form>

            {stocks.length > 0 && (
                <section className={styles.stocksSection}>
                    <h3 className={styles.sectionTitle}>Stocks</h3>
                    <div className={styles.stocksGrid}>
                        {stocks.map((stock) => (
                            <div key={stock.stockId} className={styles.stockCard}>
                                <div className={styles.stockInfo}>
                                    <span
                                        className={styles.colorDot}
                                        style={{ backgroundColor: stock.hexCode }}
                                        title={stock.colorName}
                                    />
                                    <div>
                                        <p className={styles.stockLabel}>{stock.colorName}</p>
                                        <p className={styles.stockSize}>{stock.sizeName}</p>
                                    </div>
                                </div>
                                <div className={styles.stockQtyRow}>
                                    <Input
                                        type="number"
                                        min={0}
                                        value={stock.quantity}
                                        onChange={(e) =>
                                            setStocks((prev) =>
                                                prev.map((s) =>
                                                    s.stockId === stock.stockId
                                                        ? { ...s, quantity: Number(e.target.value) }
                                                        : s
                                                )
                                            )
                                        }
                                        className={styles.stockInput}
                                    />
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        disabled={stock.saving}
                                        onClick={() => handleStockSave(stock)}
                                    >
                                        {stock.saving ? "Saving..." : "Save"}
                                    </Button>
                                </div>
                            </div>
                        ))}
                    </div>
                </section>
            )}
        </div>
    );
};

export default ClotheUpdateForm;