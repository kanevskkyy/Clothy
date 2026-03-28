import { Outlet } from "react-router-dom";
import styles from "./AdminLayout.module.css";

const AdminLayout = () => {
    return (
        <div className={styles.adminWrapper}>
            <Outlet />
        </div>
    );
};

export default AdminLayout;