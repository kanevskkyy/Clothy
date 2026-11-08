import { Link, useParams, useNavigate, useOutletContext } from "react-router-dom";
import { useEffect, useState } from "react";
import { ArrowLeft } from "lucide-react";
import { catalogApi } from "../../../../app/api/catalogApi";
import ClotheUpdateForm from "../../../../features/forms/clotheUpdateForm/ClotheUpdateForm.tsx";
import type { IClotheByIdDTO } from "../../../../entities/catalogService/interfaces/clothe/IClotheDetailDTO.ts";
import { toast } from "sonner";
import { getErrorMessage } from "../../../../shared/lib/errorHandler.ts";
import type { AdminLayoutContext } from "../../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import AdminCard from "../../ui/AdminCard.tsx";
import { AdminSkeleton } from "../../ui/AdminSkeleton.tsx";
import styles from "../page/AdminClothesPage.module.css";

const AdminClotheEditPage = () => {
    const { id } = useParams<{ id: string }>();
    const navigate = useNavigate();
    const { setPageHeader } = useOutletContext<AdminLayoutContext>();
    const [clothe, setClothe] = useState<IClotheByIdDTO | null>(null);
    const [isLoading, setIsLoading] = useState(true);

    useEffect(() => {
        if (!id) return;
        // eslint-disable-next-line react-hooks/set-state-in-effect -- existing loading flow, kept as is
        setIsLoading(true);
        catalogApi.getClotheByIdAsync(id)
            .then(setClothe)
            .catch((err) => toast.error(getErrorMessage(err)))
            .finally(() => setIsLoading(false));
    }, [id]);

    useEffect(() => {
        setPageHeader({ title: clothe ? `Edit: ${clothe.name}` : "Edit clothe", description: "Update item details and stock" });
    }, [clothe]);

    return (
        <div className={styles.formPage}>
            <Link to="/admin/clothes" className={styles.back}>
                <ArrowLeft size={16} aria-hidden="true" /> All clothes
            </Link>
            <AdminCard>
                {isLoading || !clothe ? (
                    <div className={styles.formSkeleton} role="status" aria-label="Loading clothe">
                        <AdminSkeleton width="40%" height={18} />
                        <AdminSkeleton height={44} />
                        <AdminSkeleton height={44} />
                        <AdminSkeleton height={120} />
                    </div>
                ) : (
                    <ClotheUpdateForm clothe={clothe} onSuccess={() => navigate("/admin/clothes")} />
                )}
            </AdminCard>
        </div>
    );
};

export default AdminClotheEditPage;
