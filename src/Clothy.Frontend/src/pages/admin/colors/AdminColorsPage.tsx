import { useEffect, useState } from "react";
import { Tag, Pencil, Trash2, Plus } from "lucide-react";
import Modal from "../../../shared/layout/Modal/Modal.tsx";
import { Helmet } from "react-helmet";
import { useOutletContext } from "react-router-dom";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import ColorForm from "../../../features/forms/colorForm/ColorForm.tsx";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import type { AdminLayoutContext } from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import styles from "../crud/CrudAdminPage.module.css";
import table from "../ui/AdminTable.module.css";
import AdminTable from "../ui/AdminTable.tsx";
import AdminCard from "../ui/AdminCard.tsx";
import AdminToolbar from "../ui/AdminToolbar.tsx";
import AdminButton from "../ui/AdminButton.tsx";
import AdminEmptyState from "../ui/AdminEmptyState.tsx";
import { AdminRowsSkeleton } from "../ui/AdminSkeleton.tsx";
import type { IColorReadDTO } from "../../../entities/catalogService/interfaces/color/IColorReadDTO.ts";

const AdminColorsPage = () => {
    const { setPageHeader } = useOutletContext<AdminLayoutContext>();
    const [colors, setColors] = useState<IColorReadDTO[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [deletingId, setDeletingId] = useState<string | null>(null);
    const [editingColor, setEditingColor] = useState<IColorReadDTO | null>(null);
    const [isCreating, setIsCreating] = useState(false);

    useEffect(() => {
        setPageHeader({ title: "Colors", description: "Manage catalog colors" });
    }, []);

    const fetchColors = async () => {
        try {
            setIsLoading(true);
            const data = await catalogApi.getAllColorsAsync();
            setColors(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchColors();
    }, []);

    const handleDelete = (id: string, name: string) => {
        toast(`Delete "${name}"?`, {
            action: {
                label: "Delete",
                onClick: async () => {
                    try {
                        setDeletingId(id);
                        await catalogApi.deleteColorAsync(id);
                        toast.success("Color deleted.");
                        fetchColors();
                    } catch (error) {
                        toast.error(getErrorMessage(error));
                    } finally {
                        setDeletingId(null);
                    }
                },
            },
        });
    };

    const handleEditClick = async (id: string) => {
        try {
            const color = await catalogApi.getColorByIdAsync(id);
            setEditingColor(color);
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
    };

    if (isLoading) {
        return (
            <AdminCard flush>
                <AdminRowsSkeleton rows={6} columns={4} label="Loading colors" />
            </AdminCard>
        );
    }

    return (
        <div>
            <Helmet>
                <title>Admin — Colors</title>
                <meta name="description" content="Manage catalog colors" />
            </Helmet>

            {isCreating && (
                <Modal title="Add color" onClose={() => setIsCreating(false)}>
                    <ColorForm
                        method="create"
                        onSuccess={() => {
                            setIsCreating(false);
                            fetchColors();
                        }}
                    />
                </Modal>
            )}

            {editingColor && (
                <Modal title="Edit color" onClose={() => setEditingColor(null)}>
                    <ColorForm
                        method="update"
                        colorId={editingColor.id}
                        initialData={{
                            name: editingColor.name,
                            slug: editingColor.slug,
                            hexCode: editingColor.hexCode,
                        }}
                        onSuccess={() => {
                            setEditingColor(null);
                            fetchColors();
                        }}
                    />
                </Modal>
            )}

            <AdminToolbar
                start={<span className={styles.count}>{colors.length} colors</span>}
                end={
                    <AdminButton variant="primary" size="md" icon={<Plus size={15} />} onClick={() => setIsCreating(true)}>
                        Add color
                    </AdminButton>
                }
            />

            <AdminCard flush aria-label="Colors">
                {colors.length === 0 ? (
                    <AdminEmptyState
                        icon={<Tag size={22} />}
                        title="No colors yet"
                        description="Add your first color to get started."
                    />
                ) : (
                    <AdminTable caption="Colors">
                        <thead>
                        <tr>
                            <th scope="col">Name</th>
                            <th scope="col">Slug</th>
                            <th scope="col">Color</th>
                            <th scope="col" className={table.actions}>Actions</th>
                        </tr>
                        </thead>
                        <tbody>
                        {colors.map((color) => (
                            <tr key={color.id}>
                                <td className={styles.name}>{color.name}</td>
                                <td><span className={styles.slug}>{color.slug}</span></td>
                                <td>
                                    <span className={styles.swatch}>
                                        <span className={styles.swatchDot} style={{ backgroundColor: color.hexCode }} aria-hidden="true" />
                                        {color.hexCode}
                                    </span>
                                </td>
                                <td className={table.actions}>
                                    <div className={table.actionsInner}>
                                        <AdminButton
                                            variant="quiet"
                                            icon={<Pencil size={14} />}
                                            aria-label={`Edit ${color.name}`}
                                            onClick={() => handleEditClick(color.id)}
                                        >
                                            <span className={styles.actionLabel}>Edit</span>
                                        </AdminButton>
                                        <AdminButton
                                            variant="danger"
                                            icon={<Trash2 size={14} />}
                                            aria-label={`Delete ${color.name}`}
                                            disabled={deletingId === color.id}
                                            onClick={() => handleDelete(color.id, color.name)}
                                        >
                                            <span className={styles.actionLabel}>Delete</span>
                                        </AdminButton>
                                    </div>
                                </td>
                            </tr>
                        ))}
                        </tbody>
                    </AdminTable>
                )}
            </AdminCard>
        </div>
    );
};

export default AdminColorsPage;