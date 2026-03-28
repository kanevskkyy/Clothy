import { Pencil, Trash2, Plus } from "lucide-react";
import EmptyState from "../../../shared/ui/EmptyState/EmptyState.tsx";
import Loader from "../../../shared/ui/Loader/Loader.tsx";
import Modal from "../../../shared/layout/Modal/Modal.tsx";
import Button from "../../../shared/ui/Button/Button.tsx";
import { Helmet } from "react-helmet";
import { useOutletContext } from "react-router-dom";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import styles from "./CrudAdminPage.module.css";
import type { IBaseCrudEntity } from "../../../entities/catalogService/interfaces/shared/IBaseCrudEntity.ts";
import type {AdminLayoutContext} from "../../../features/auth/admin/adminLayout/AdminLayout.tsx";

interface CrudAdminPageProps<T extends IBaseCrudEntity> {
    title: string;
    description: string;
    entityName: string;
    entityNamePlural: string;

    icon: React.ReactNode;

    getAll: () => Promise<T[]>;
    getById: (id: string) => Promise<T>;
    create: (data: any) => Promise<T>;
    update: (id: string, data: any) => Promise<T>;
    remove: (id: string) => Promise<void>;

    FormComponent: React.ComponentType<any>;
}

function CrudAdminPage<T extends IBaseCrudEntity>({
                                                     title,
                                                     description,
                                                     entityName,
                                                     entityNamePlural,
                                                     icon,
                                                     getAll,
                                                     getById,
                                                     remove,
                                                     FormComponent,
                                                 }: CrudAdminPageProps<T>) {
    const { setPageHeader } = useOutletContext<AdminLayoutContext>();

    const [items, setItems] = useState<T[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [deletingId, setDeletingId] = useState<string | null>(null);
    const [editingItem, setEditingItem] = useState<T | null>(null);
    const [isCreating, setIsCreating] = useState(false);

    useEffect(() => {
        setPageHeader({ title, description });
    }, []);

    const fetchItems = async () => {
        try {
            setIsLoading(true);
            const data = await getAll();
            setItems(data);
        } catch (error) {
            toast.error(getErrorMessage(error));
        } finally {
            setIsLoading(false);
        }
    };

    useEffect(() => {
        fetchItems();
    }, []);

    const handleDelete = (id: string, name: string) => {
        toast(`Delete "${name}"?`, {
            action: {
                label: "Delete",
                onClick: async () => {
                    try {
                        setDeletingId(id);
                        await remove(id);
                        toast.success(`${entityName} deleted.`);
                        fetchItems();
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
            const item = await getById(id);
            setEditingItem(item);
        } catch (error) {
            toast.error(getErrorMessage(error));
        }
    };

    if (isLoading) return <Loader />;

    return (
        <div>
            <Helmet>
                <title>Admin — {entityNamePlural}</title>
                <meta name="description" content={description} />
            </Helmet>

            {isCreating && (
                <Modal
                    title={`Add ${entityName.toLowerCase()}`}
                    onClose={() => setIsCreating(false)}
                >
                    <FormComponent
                        method="create"
                        onSuccess={() => {
                            setIsCreating(false);
                            fetchItems();
                        }}
                    />
                </Modal>
            )}

            {editingItem && (
                <Modal
                    title={`Edit ${entityName.toLowerCase()}`}
                    onClose={() => setEditingItem(null)}
                >
                    <FormComponent
                        method="update"
                        {...{
                            [`${entityName.toLowerCase()}Id`]: editingItem.id,
                        }}
                        initialData={{
                            name: editingItem.name,
                            slug: editingItem.slug,
                        }}
                        onSuccess={() => {
                            setEditingItem(null);
                            fetchItems();
                        }}
                    />
                </Modal>
            )}

            <div className={styles.header}>
                <Button
                    variant="primary"
                    size="md"
                    icon={<Plus size={15} />}
                    onClick={() => setIsCreating(true)}
                >
                    Add {entityName.toLowerCase()}
                </Button>
            </div>

            {items.length === 0 ? (
                <EmptyState
                    icon={icon}
                    title={`No ${entityNamePlural.toLowerCase()} yet`}
                    description={`Add your first ${entityName.toLowerCase()} to get started.`}
                    buttons={[]}
                />
            ) : (
                <div className={styles.list}>
                    {items.map((item) => (
                        <div key={item.id} className={styles.row}>
                            <div className={styles.rowInfo}>
                                <span className={styles.rowName}>{item.name}</span>
                                <span className={styles.rowSlug}>{item.slug ?? ""}</span>
                            </div>

                            <div className={styles.rowActions}>
                                <Button
                                    variant="secondary"
                                    size="sm"
                                    icon={<Pencil size={14} />}
                                    onClick={() => handleEditClick(item.id)}
                                >
                                    Edit
                                </Button>

                                <Button
                                    variant="secondary"
                                    size="sm"
                                    icon={<Trash2 size={14} />}
                                    disabled={deletingId === item.id}
                                    onClick={() => handleDelete(item.id, item.name)}
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
}

export default CrudAdminPage;