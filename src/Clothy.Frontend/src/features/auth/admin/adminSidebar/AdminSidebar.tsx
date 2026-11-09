import {Link, useLocation, useNavigate} from "react-router-dom";
import {
    LayoutDashboard, ShoppingBag, Package, Tag, Layers,
    Palette, Hash, Ruler, Truck, MessageSquare, HelpCircle, X,
    LogOut
} from "lucide-react";
import styles from "./AdminSidebar.module.css";
import {useAuthStore} from "../../../../app/stores/authStore.ts";
import {authApi} from "../../../../app/api/authApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../../../shared/lib/errorHandler.ts";

interface IMenuItem {
    to: string;
    label: string;
    icon: React.ReactNode;
}

interface IMenuGroup {
    title: string;
    items: IMenuItem[];
}

interface AdminSidebarProps {
    isOpen: boolean;
    onClose: () => void;
}

const ICON_SIZE = 18;

const AdminSidebar = ({ isOpen, onClose }: AdminSidebarProps) => {
    const location = useLocation();
    const navigate = useNavigate();

    const menuGroups: IMenuGroup[] = [
        {
            title: "Overview",
            items: [
                {to: "/admin", label: "Dashboard", icon: <LayoutDashboard size={ICON_SIZE}/>},
            ],
        },
        {
            title: "Sales",
            items: [
                {to: "/admin/orders", label: "Orders", icon: <Package size={ICON_SIZE}/>},
            ],
        },
        {
            title: "Catalog",
            items: [
                {to: "/admin/clothes", label: "Clothes", icon: <ShoppingBag size={ICON_SIZE}/>},
                {to: "/admin/brands", label: "Brands", icon: <Tag size={ICON_SIZE}/>},
                {to: "/admin/collections", label: "Collections", icon: <Layers size={ICON_SIZE}/>},
                {to: "/admin/colors", label: "Colors", icon: <Palette size={ICON_SIZE}/>},
                {to: "/admin/tags", label: "Tags", icon: <Hash size={ICON_SIZE}/>},
                {to: "/admin/sizes", label: "Sizes", icon: <Ruler size={ICON_SIZE}/>},
                {to: "/admin/delivery", label: "Delivery", icon: <Truck size={ICON_SIZE}/>},
            ],
        },
        {
            title: "Moderation",
            items: [
                {to: "/admin/reviews", label: "Reviews", icon: <MessageSquare size={ICON_SIZE}/>},
                {to: "/admin/questions", label: "Questions", icon: <HelpCircle size={ICON_SIZE}/>},
            ],
        },
    ];

    const isActive = (path: string) => {
        if (path === "/admin") return location.pathname === "/admin";
        return location.pathname.startsWith(path);
    };

    const handleLogout = () => {
        useAuthStore.getState().logout();
        navigate("/login", { replace: true });
        authApi.logoutAsync().catch((error) => {
            toast.error(getErrorMessage(error));
        });
    };

    return (
        <aside
            id="admin-sidebar"
            className={`${styles.sidebar} ${isOpen ? styles.open : ""}`}
            aria-label="Admin navigation"
        >
            <div className={styles.header}>
                <div className={styles.brand}>
                    <span className={styles.logo}>Clothy</span>
                    <span className={styles.logoShort} aria-hidden="true">C</span>
                    <span className={styles.subtitle}>Admin dashboard</span>
                </div>
                <button type="button" className={styles.closeButton} onClick={onClose} aria-label="Close menu">
                    <X size={20} aria-hidden="true"/>
                </button>
            </div>

            <nav className={styles.menu}>
                {menuGroups.map((group) => (
                    <div key={group.title} className={styles.group}>
                        <span className={styles.groupTitle}>{group.title}</span>
                        {group.items.map((item) => {
                            const active = isActive(item.to);
                            return (
                                <Link
                                    key={item.to}
                                    to={item.to}
                                    className={`${styles.item} ${active ? styles.active : ""}`}
                                    aria-current={active ? "page" : undefined}
                                    title={item.label}
                                    onClick={onClose}
                                >
                                    <span className={styles.icon} aria-hidden="true">{item.icon}</span>
                                    <span className={styles.label}>{item.label}</span>
                                </Link>
                            );
                        })}
                    </div>
                ))}
            </nav>

            <div className={styles.footer}>
                <button type="button" className={styles.logout} onClick={handleLogout} title="Logout">
                    <span className={styles.icon} aria-hidden="true"><LogOut size={ICON_SIZE}/></span>
                    <span className={styles.label}>Logout</span>
                </button>
            </div>
        </aside>
    );
};

export default AdminSidebar;
