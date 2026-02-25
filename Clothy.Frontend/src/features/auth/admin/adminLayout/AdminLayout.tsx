import {useState, useEffect} from "react";
import {Outlet} from "react-router-dom";
import styles from "./AdminLayout.module.css";
import AdminSidebar from "../adminSidebar/AdminSidebar.tsx";
import {Menu} from "lucide-react";

export interface AdminLayoutContext {
    setPageHeader: (header: { title: string; description?: string }) => void;
}

const AdminLayout = () => {
    const [isSidebarOpen, setIsSidebarOpen] = useState(false);
    const [pageHeader, setPageHeader] = useState<{ title: string; description?: string } | null>(null);

    useEffect(() => {
        if (isSidebarOpen) {
            const scrollY = window.scrollY;
            document.body.style.position = "fixed";
            document.body.style.top = `-${scrollY}px`;
            document.body.style.width = "100%";
        } else {
            const scrollY = document.body.style.top;
            document.body.style.position = "";
            document.body.style.top = "";
            document.body.style.width = "";
            if (scrollY) window.scrollTo(0, parseInt(scrollY) * -1);
        }
        return () => {
            document.body.style.position = "";
            document.body.style.top = "";
            document.body.style.width = "";
        };
    }, [isSidebarOpen]);

    return (
        <div className={styles.container}>
            {isSidebarOpen && (
                <div className={styles.overlay} onClick={() => setIsSidebarOpen(false)}/>
            )}

            <AdminSidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)}/>

            <main className={styles.main}>
                <div className={styles.topBar}>
                    <button className={styles.menuButton} onClick={() => setIsSidebarOpen(true)}>
                        <Menu size={22}/>
                    </button>

                    {pageHeader && (
                        <div className={styles.pageHeader}>
                            <h2 className={styles.pageTitle}>{pageHeader.title}</h2>
                            {pageHeader.description && (
                                <p className={styles.pageDescription}>{pageHeader.description}</p>
                            )}
                        </div>
                    )}
                </div>

                <div className={styles.content}>
                    <Outlet context={{setPageHeader} satisfies AdminLayoutContext}/>
                </div>
            </main>
        </div>
    );
};

export default AdminLayout;