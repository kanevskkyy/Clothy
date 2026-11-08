import { useEffect } from "react";
import { Link, useNavigate, useOutletContext } from "react-router-dom";
import { ArrowLeft } from "lucide-react";
import ClotheCreateForm from "../../../../features/forms/clotheCreateForm/ClotheCreateForm.tsx";
import type { AdminLayoutContext } from "../../../../features/auth/admin/adminLayout/AdminLayout.tsx";
import AdminCard from "../../ui/AdminCard.tsx";
import styles from "../page/AdminClothesPage.module.css";

const AdminClotheCreatePage = () => {
    const navigate = useNavigate();
    const { setPageHeader } = useOutletContext<AdminLayoutContext>();

    useEffect(() => {
        setPageHeader({ title: "Create Clothe", description: "Add a new item to the catalog" });
    }, []);

    return (
        <div className={styles.formPage}>
            <Link to="/admin/clothes" className={styles.back}>
                <ArrowLeft size={16} aria-hidden="true" /> All clothes
            </Link>
            <AdminCard>
                <ClotheCreateForm onSuccess={() => navigate("/admin/clothes")} />
            </AdminCard>
        </div>
    );
};

export default AdminClotheCreatePage;
