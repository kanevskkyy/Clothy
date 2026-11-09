import {useState, useEffect} from "react";
import {Outlet} from "react-router-dom";
import styles from "./AdminLayout.module.css";
import AdminSidebar from "../adminSidebar/AdminSidebar.tsx";
import {Menu} from "lucide-react";
import themeStyles from "../../../../pages/admin/ui/AdminTheme.module.css";
import AdminPageHeader from "../../../../pages/admin/ui/AdminPageHeader.tsx";

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

    // keyboard: Escape closes the mobile menu
    useEffect(() => {
        if (!isSidebarOpen) return;
        const onKeyDown = (e: KeyboardEvent) => {
            if (e.key === "Escape") setIsSidebarOpen(false);
        };
        window.addEventListener("keydown", onKeyDown);
        return () => window.removeEventListener("keydown", onKeyDown);
    }, [isSidebarOpen]);

    return (
        <div className={`${themeStyles.root} ${styles.container}`}>
            <a href="#admin-content" className={styles.skipLink}>Skip to content</a>

            {isSidebarOpen && (
                <div className={styles.overlay} onClick={() => setIsSidebarOpen(false)} aria-hidden="true"/>
            )}

            <AdminSidebar isOpen={isSidebarOpen} onClose={() => setIsSidebarOpen(false)}/>

            <main className={styles.main}>
                <div className={styles.topBar}>
                    <button
                        type="button"
                        className={styles.menuButton}
                        onClick={() => setIsSidebarOpen(true)}
                        aria-label="Open menu"
                        aria-expanded={isSidebarOpen}
                        aria-controls="admin-sidebar"
                    >
                        <Menu size={20} aria-hidden="true"/>
                    </button>

                    {pageHeader && (
                        <AdminPageHeader title={pageHeader.title} description={pageHeader.description}/>
                    )}
                </div>

                <div id="admin-content" className={styles.content} tabIndex={-1}>
                    <Outlet context={{setPageHeader} satisfies AdminLayoutContext}/>
                </div>
            </main>
        </div>
    );
};

export default AdminLayout;
