import { Pencil, Trash2, Plus } from "lucide-react";
import Modal from "../../../shared/layout/Modal/Modal.tsx";
import { Helmet } from "react-helmet";
import { useOutletContext } from "react-router-dom";
import { useEffect, useState } from "react";
import { toast } from "sonner";
import { getErrorMessage } from "../../../shared/lib/errorHandler.ts";
import styles from "./CrudAdminPage.module.css";
import table from "../ui/AdminTable.module.css";
import AdminTable from "../ui/AdminTable.tsx";
import AdminCard from "../ui/AdminCard.tsx";
import AdminToolbar from "../ui/AdminToolbar.tsx";
import AdminButton from "../ui/AdminButton.tsx";
import AdminEmptyState from "../ui/AdminEmptyState.tsx";
import { AdminRowsSkeleton } from "../ui/AdminSkeleton.tsx";
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
    // eslint-disable-next-line @typescript-eslint/no-explicit-any -- existing prop type, kept as is
    create: (data: any) => Promise<T>;
    // eslint-disable-next-line @typescript-eslint/no-explicit-any -- existing prop type, kept as is
    update: (id: string, data: any) => Promise<T>;
    remove: (id: string) => Promise<void>;

    // eslint-disable-next-line @typescript-eslint/no-explicit-any -- existing prop type, kept as is
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

    if (isLoading) {
        return (
            <AdminCard flush>
                <AdminRowsSkeleton rows={6} columns={3} label={`Loading ${entityNamePlural.toLowerCase()}`} />
            </AdminCard>
        );
    }

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

            <AdminToolbar
                start={<span className={styles.count}>{items.length} {entityNamePlural.toLowerCase()}</span>}
                end={
                    <AdminButton
                        variant="primary"
                        size="md"
                        icon={<Plus size={15} />}
                        onClick={() => setIsCreating(true)}
                    >
                        Add {entityName.toLowerCase()}
                    </AdminButton>
                }
            />

            <AdminCard flush aria-label={entityNamePlural}>
                {items.length === 0 ? (
                    <AdminEmptyState
                        icon={icon}
                        title={`No ${entityNamePlural.toLowerCase()} yet`}
                        description={`Add your first ${entityName.toLowerCase()} to get started.`}
                    />
                ) : (
                    <AdminTable caption={entityNamePlural}>
                        <thead>
                        <tr>
                            <th scope="col">Name</th>
                            <th scope="col">Slug</th>
                            <th scope="col" className={table.actions}>Actions</th>
                        </tr>
                        </thead>
                        <tbody>
                        {items.map((item) => (
                            <tr key={item.id}>
                                <td className={styles.name}>{item.name}</td>
                                <td>{item.slug ? <span className={styles.slug}>{item.slug}</span> : <span className={table.muted}>—</span>}</td>
                                <td className={table.actions}>
                                    <div className={table.actionsInner}>
                                        <AdminButton
                                            variant="quiet"
                                            icon={<Pencil size={14} />}
                                            aria-label={`Edit ${item.name}`}
                                            onClick={() => handleEditClick(item.id)}
                                        >
                                            <span className={styles.actionLabel}>Edit</span>
                                        </AdminButton>
                                        <AdminButton
                                            variant="danger"
                                            icon={<Trash2 size={14} />}
                                            aria-label={`Delete ${item.name}`}
                                            disabled={deletingId === item.id}
                                            onClick={() => handleDelete(item.id, item.name)}
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
}

export default CrudAdminPage;