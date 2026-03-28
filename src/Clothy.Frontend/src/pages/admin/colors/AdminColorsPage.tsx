import { useEffect, useState } from "react";
import { Tag, Pencil, Trash2, Plus } from "lucide-react";
import EmptyState from "../../../shared/ui/EmptyState/EmptyState.tsx";
import Loader from "../../../shared/ui/Loader/Loader.tsx";
import Modal from "../../../shared/layout/Modal/Modal.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import { Helmet } from "react-helmet";
import { useOutletContext } from "react-router-dom";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import ColorForm from "../../../features/forms/colorForm/ColorForm.tsx";
import { catalogApi } from "../../../app/api/catalogApi.ts";
import type { AdminLayoutContext } from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import styles from "./AdminColorsPage.module.css";
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

    if (isLoading) return <Loader />;

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

            <div className={styles.header}>
                <Button variant="primary" size="md" icon={<Plus size={15} />} onClick={() => setIsCreating(true)}>
                    Add color
                </Button>
            </div>

            {colors.length === 0 ? (
                <EmptyState
                    icon={<Tag size={28} color="#6B6B6B" />}
                    title="No colors yet"
                    description="Add your first color to get started."
                    buttons={[]}
                />
            ) : (
                <div className={styles.list}>
                    {colors.map((color) => (
                        <div key={color.id} className={styles.row}>
                            <div className={styles.rowInfo}>
                                <span className={styles.rowName}>{color.name}</span>
                                <span className={styles.rowSlug}>{color.slug}</span>
                                <span
                                    className={styles.colorCircle}
                                    style={{ backgroundColor: color.hexCode }}
                                    title={color.hexCode}
                                />
                            </div>

                            <div className={styles.rowActions}>
                                <Button
                                    variant="secondary"
                                    size="sm"
                                    icon={<Pencil size={14} />}
                                    onClick={() => handleEditClick(color.id)}
                                >
                                    Edit
                                </Button>

                                <Button
                                    variant="secondary"
                                    size="sm"
                                    icon={<Trash2 size={14} />}
                                    disabled={deletingId === color.id}
                                    onClick={() => handleDelete(color.id, color.name)}
                                >
                                    Delete
                                </Button>
                            </div>
                        </div>
                    ))}
                </div>
            )}
        </div>
    );
};

export default AdminColorsPage;