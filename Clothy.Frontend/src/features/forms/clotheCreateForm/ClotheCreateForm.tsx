import React, { useRef, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { Plus, Trash2 } from "lucide-react";
import { toast } from "sonner";
import { nanoid } from "nanoid";
import FormField from "../../../shared/form/FormField/FormField.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import Input from "../../../shared/ui/Input/Input.tsx";
import Textarea from "../../../shared/ui/Textarea/Textarea.tsx";
import Select from "../../../shared/ui/Select/Select.tsx";
import RadioOption from "../../../shared/ui/RadioOption/RadioOption.tsx";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import { clotheCreateSchema } from "../../../app/schemas/clotheCreateSchema.ts";
import { getZodFieldErrors } from "../../../shared/lib/getZodFieldErrors.ts";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import styles from "./ClotheCreateForm.module.css";

type PhotoInGroup = {
    localId: string;
    file: File | null;
    isMain: boolean;
};

type ColorGroup = {
    localId: string;
    colorId: string;
    photos: PhotoInGroup[];
};

type MaterialEntry = {
    localId: string;
    materialId: string;
    percentage: number | "";
};

type FormData = {
    name: string;
    slug: string;
    description: string;
    price: number | "";
    gender: "Male" | "Female" | "Unisex" | "";
    brandId: string;
    clothingTypeId: string;
    collectionId: string;
    tagIds: string[];
    colorGroups: ColorGroup[];
    materials: MaterialEntry[];
};

interface Props {
    onSuccess?: () => void;
}

const ClotheCreateForm = ({ onSuccess }: Props) => {
    const [formData, setFormData] = useState<FormData>({
        name: "",
        slug: "",
        description: "",
        price: "",
        gender: "",
        brandId: "",
        clothingTypeId: "",
        collectionId: "",
        tagIds: [],
        colorGroups: [
            {
                localId: nanoid(),
                colorId: "",
                photos: [{ localId: nanoid(), file: null, isMain: true }],
            },
        ],
        materials: [],
    });

    const [errors, setErrors] = useState<Record<string, string>>({});
    const [isSubmitting, setIsSubmitting] = useState(false);
    const fileInputRefs = useRef<Record<string, HTMLInputElement | null>>({});

    const { data: filters } = useQuery({
        queryKey: ["catalog-filters"],
        queryFn: () => catalogApi.getFiltersAsync(),
        staleTime: 1000 * 60 * 15,
    });

    const brandOptions = filters?.brands.map((b) => ({ value: b.id, label: b.name })) ?? [];
    const clothingTypeOptions = filters?.clothingTypes.map((c) => ({ value: c.id, label: c.name })) ?? [];
    const collectionOptions = filters?.collections.map((c) => ({ value: c.id, label: c.name })) ?? [];
    const tagOptions = filters?.tags.map((t) => ({ value: t.id, label: t.name })) ?? [];
    const colorOptions = filters?.colors.map((c) => ({ value: c.id, label: c.name })) ?? [];
    const materialOptions = filters?.materials.map((m) => ({ value: m.id, label: m.name })) ?? [];

    const clearError = (key: string) => setErrors((prev) => { const n = { ...prev }; delete n[key]; return n; });

    const handleField = (field: keyof FormData, value: unknown) => {
        setFormData((prev) => ({ ...prev, [field]: value }));
        clearError(field);
    };

    const autoSlug = (name: string) =>
        name
            .toLowerCase()
            .replace(/\s+/g, "-")
            .replace(/[^a-z0-9-]/g, "")
            .replace(/-+/g, "-")
            .replace(/^-|-$/g, "")
            .replace(/^\d+/, "");

    const addColorGroup = () => {
        setFormData((prev) => ({
            ...prev,
            colorGroups: [
                ...prev.colorGroups,
                {
                    localId: nanoid(),
                    colorId: "",
                    photos: [{ localId: nanoid(), file: null, isMain: true }],
                },
            ],
        }));
    };

    const removeColorGroup = (groupId: string) => {
        setFormData((prev) => ({
            ...prev,
            colorGroups: prev.colorGroups.filter((g) => g.localId !== groupId),
        }));
    };

    const updateGroupColor = (groupId: string, colorId: string) => {
        setFormData((prev) => ({
            ...prev,
            colorGroups: prev.colorGroups.map((g) =>
                g.localId === groupId ? { ...g, colorId } : g
            ),
        }));
    };

    const addPhotoToGroup = (groupId: string) => {
        setFormData((prev) => ({
            ...prev,
            colorGroups: prev.colorGroups.map((g) =>
                g.localId === groupId
                    ? { ...g, photos: [...g.photos, { localId: nanoid(), file: null, isMain: false }] }
                    : g
            ),
        }));
    };

    const removePhotoFromGroup = (groupId: string, photoId: string) => {
        setFormData((prev) => ({
            ...prev,
            colorGroups: prev.colorGroups.map((g) => {
                if (g.localId !== groupId) return g;
                const remaining = g.photos.filter((p) => p.localId !== photoId);
                const hasMain = remaining.some((p) => p.isMain);
                return {
                    ...g,
                    photos: hasMain
                        ? remaining
                        : remaining.map((p, i) => ({ ...p, isMain: i === 0 })),
                };
            }),
        }));
    };

    const setMainPhoto = (groupId: string, photoId: string) => {
        setFormData((prev) => ({
            ...prev,
            colorGroups: prev.colorGroups.map((g) =>
                g.localId !== groupId
                    ? g
                    : {
                        ...g,
                        photos: g.photos.map((p) => ({ ...p, isMain: p.localId === photoId })),
                    }
            ),
        }));
    };

    const setPhotoFile = (groupId: string, photoId: string, file: File) => {
        setFormData((prev) => ({
            ...prev,
            colorGroups: prev.colorGroups.map((g) =>
                g.localId !== groupId
                    ? g
                    : { ...g, photos: g.photos.map((p) => (p.localId === photoId ? { ...p, file } : p)) }
            ),
        }));
    };

    const addMaterial = () =>
        setFormData((prev) => ({
            ...prev,
            materials: [...prev.materials, { localId: nanoid(), materialId: "", percentage: "" }],
        }));

    const removeMaterial = (localId: string) =>
        setFormData((prev) => ({
            ...prev,
            materials: prev.materials.filter((m) => m.localId !== localId),
        }));

    const updateMaterial = (localId: string, field: keyof MaterialEntry, value: unknown) =>
        setFormData((prev) => ({
            ...prev,
            materials: prev.materials.map((m) => (m.localId === localId ? { ...m, [field]: value } : m)),
        }));

    const totalPercentage = formData.materials.reduce(
        (sum, m) => sum + (typeof m.percentage === "number" ? m.percentage : 0),
        0
    );

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const additionalPhotos = formData.colorGroups.flatMap((g) =>
            g.photos.map((p) => ({ photo: p.file, colorId: g.colorId, isMain: p.isMain }))
        );

        const parseResult = clotheCreateSchema.safeParse({
            name: formData.name,
            slug: formData.slug,
            description: formData.description,
            price: formData.price === "" ? undefined : Number(formData.price),
            gender: formData.gender || undefined,
            brandId: formData.brandId,
            clothingTypeId: formData.clothingTypeId,
            collectionId: formData.collectionId,
            tagIds: formData.tagIds,
            additionalPhotos,
            materials: formData.materials.map((m) => ({
                materialId: m.materialId,
                percentage: typeof m.percentage === "number" ? m.percentage : 0,
            })),
        });

        if (!parseResult.success) {
            setErrors(getZodFieldErrors(parseResult.error));
            return;
        }

        const fd = new FormData();
        fd.append("Name", parseResult.data.name);
        fd.append("Slug", parseResult.data.slug);
        fd.append("Description", parseResult.data.description);
        fd.append("Price", parseResult.data.price.toFixed(2).replace(".", ","));
        fd.append("Gender", parseResult.data.gender);
        fd.append("BrandId", parseResult.data.brandId);
        fd.append("ClothingTypeId", parseResult.data.clothingTypeId);
        fd.append("CollectionId", parseResult.data.collectionId);
        parseResult.data.tagIds.forEach((id) => fd.append("TagIds", id));
        parseResult.data.additionalPhotos.forEach((p, i) => {
            fd.append(`AdditionalPhotos[${i}].Photo`, p.photo);
            fd.append(`AdditionalPhotos[${i}].ColorId`, p.colorId);
            fd.append(`AdditionalPhotos[${i}].IsMain`, p.isMain.toString());
        });
        if (parseResult.data.materials.length > 0) {
            fd.append("Materials", JSON.stringify(parseResult.data.materials));
        }

        console.log("=== FormData payload ===");
        fd.forEach((value, key) => console.log(`${key}:`, value));

        setIsSubmitting(true);
        try {
            await catalogApi.createClotheAsync(fd);
            toast.success("Clothe created successfully.");
            onSuccess?.();
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
        <form className={styles.form} onSubmit={handleSubmit}>

            <section className={styles.section}>
                <h3 className={styles.sectionTitle}>Basic Info</h3>
                <div className={styles.row2}>
                    <FormField label="Name" htmlFor="name" required error={errors.name}>
                        <Input
                            id="name"
                            placeholder="Cool T-Shirt"
                            value={formData.name}
                            onChange={(e) => {
                                handleField("name", e.target.value);
                                if (!formData.slug) handleField("slug", autoSlug(e.target.value));
                            }}
                            error={!!errors.name}
                        />
                    </FormField>
                    <FormField label="Slug" htmlFor="slug" required error={errors.slug}>
                        <Input
                            id="slug"
                            placeholder="cool-t-shirt-2024"
                            value={formData.slug}
                            onChange={(e) => handleField("slug", e.target.value)}
                            error={!!errors.slug}
                        />
                    </FormField>
                </div>

                <FormField label="Description" htmlFor="description" required error={errors.description}>
                    <Textarea
                        id="description"
                        placeholder="A comfortable cotton t-shirt..."
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
                        placeholder="99.99"
                        value={formData.price}
                        onChange={(e) => handleField("price", e.target.value === "" ? "" : parseFloat(e.target.value))}
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

                <FormField label="Tags" htmlFor="tagIds" required error={errors.tagIds}>
                    <Select
                        inputId="tagIds"
                        placeholder="Select tags"
                        isMulti
                        options={tagOptions}
                        value={tagOptions.filter((o) => formData.tagIds.includes(o.value))}
                        onChange={(opts: any) => handleField("tagIds", (opts ?? []).map((o: any) => o.value))}
                        error={!!errors.tagIds}
                        menuPortalTarget={document.body}
                        menuPosition="fixed"
                    />
                </FormField>
            </section>

            <section className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h3 className={styles.sectionTitle}>Photos</h3>
                    <Button type="button" variant="outline" size="sm" icon={<Plus size={14} />} onClick={addColorGroup}>
                        Add color
                    </Button>
                </div>
                {errors.additionalPhotos && <p className={styles.sectionError}>{errors.additionalPhotos}</p>}

                <div className={styles.colorGroupsList}>
                    {formData.colorGroups.map((group, groupIdx) => (
                        <div key={group.localId} className={styles.colorGroup}>
                            <div className={styles.colorGroupHeader}>
                                <div className={styles.colorGroupSelect}>
                                    <Select
                                        placeholder="Select color"
                                        options={colorOptions}
                                        value={colorOptions.find((o) => o.value === group.colorId) || null}
                                        onChange={(opt: any) => updateGroupColor(group.localId, opt?.value ?? "")}
                                        error={!!errors[`additionalPhotos.${groupIdx}.colorId`]}
                                        menuPortalTarget={document.body}
                                        menuPosition="fixed"
                                    />
                                </div>
                                <div className={styles.colorGroupActions}>
                                    <Button
                                        type="button"
                                        variant="outline"
                                        size="sm"
                                        icon={<Plus size={13} />}
                                        onClick={() => addPhotoToGroup(group.localId)}
                                    >
                                        Add photo
                                    </Button>
                                    {formData.colorGroups.length > 1 && (
                                        <button
                                            type="button"
                                            className={styles.removeBtn}
                                            onClick={() => removeColorGroup(group.localId)}
                                        >
                                            <Trash2 size={15} />
                                        </button>
                                    )}
                                </div>
                            </div>

                            <div className={styles.photosList}>
                                {group.photos.map((photo, photoIdx) => {
                                    const refKey = `${group.localId}-${photo.localId}`;
                                    return (
                                        <div key={photo.localId} className={styles.photoRow}>
                                            <span className={styles.photoIndex}>{photoIdx + 1}</span>

                                            <input
                                                ref={(el) => { fileInputRefs.current[refKey] = el; }}
                                                type="file"
                                                accept=".jpg,.jpeg,.png,.gif,.svg,.webp"
                                                className={styles.hiddenInput}
                                                onChange={(e) => {
                                                    const file = e.target.files?.[0];
                                                    if (file) setPhotoFile(group.localId, photo.localId, file);
                                                }}
                                            />

                                            <div className={styles.photoRowContent}>
                                                <div className={styles.fileRow}>
                                                    <Button
                                                        type="button"
                                                        variant="outline"
                                                        size="sm"
                                                        onClick={() => fileInputRefs.current[refKey]?.click()}
                                                    >
                                                        Choose file
                                                    </Button>
                                                    <span className={styles.fileName}>
                                                        {photo.file ? photo.file.name : "No file chosen"}
                                                    </span>
                                                </div>

                                                <label className={styles.mainLabel}>
                                                    <input
                                                        type="radio"
                                                        name={`main-${group.localId}`}
                                                        checked={photo.isMain}
                                                        onChange={() => setMainPhoto(group.localId, photo.localId)}
                                                    />
                                                    Main
                                                </label>
                                            </div>

                                            {group.photos.length > 1 && (
                                                <button
                                                    type="button"
                                                    className={styles.removeBtn}
                                                    onClick={() => removePhotoFromGroup(group.localId, photo.localId)}
                                                >
                                                    <Trash2 size={14} />
                                                </button>
                                            )}
                                        </div>
                                    );
                                })}
                            </div>
                        </div>
                    ))}
                </div>
            </section>

            <section className={styles.section}>
                <div className={styles.sectionHeader}>
                    <h3 className={styles.sectionTitle}>
                        Materials
                        {formData.materials.length > 0 && (
                            <span className={`${styles.percentageBadge} ${totalPercentage === 100 ? styles.percentageOk : styles.percentageWarn}`}>
                                {totalPercentage}%
                            </span>
                        )}
                    </h3>
                    <Button type="button" variant="outline" size="sm" icon={<Plus size={14} />} onClick={addMaterial}>
                        Add material
                    </Button>
                </div>
                {errors.materials && <p className={styles.sectionError}>{errors.materials}</p>}

                {formData.materials.length > 0 && (
                    <div className={styles.entriesList}>
                        {formData.materials.map((mat) => (
                            <div key={mat.localId} className={styles.entryRow}>
                                <div className={styles.entryFields}>
                                    <Select
                                        placeholder="Select material"
                                        options={materialOptions}
                                        value={materialOptions.find((o) => o.value === mat.materialId) || null}
                                        onChange={(opt: any) => updateMaterial(mat.localId, "materialId", opt?.value ?? "")}
                                        menuPortalTarget={document.body}
                                        menuPosition="fixed"
                                    />
                                    <Input
                                        type="number"
                                        min={0}
                                        max={100}
                                        placeholder="%"
                                        value={mat.percentage}
                                        onChange={(e) =>
                                            updateMaterial(mat.localId, "percentage", e.target.value === "" ? "" : Number(e.target.value))
                                        }
                                        className={styles.percentInput}
                                    />
                                </div>
                                <button type="button" className={styles.removeBtn} onClick={() => removeMaterial(mat.localId)}>
                                    <Trash2 size={16} />
                                </button>
                            </div>
                        ))}
                    </div>
                )}
            </section>

            <Button disabled={isSubmitting} type="submit" variant="primary" size="lg" fullWidth>
                {isSubmitting ? "Creating..." : "Create clothe"}
            </Button>
        </form>
    );
};

export default ClotheCreateForm;