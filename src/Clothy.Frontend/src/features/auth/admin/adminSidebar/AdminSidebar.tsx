import {Link, useLocation, useNavigate} from "react-router-dom";
import {
    LayoutDashboard, ShoppingBag, Package, Tag, Layers,
    Palette, Hash, Ruler, Truck, MessageSquare, HelpCircle, X,
    LogOut
} from "lucide-react";
import styles from "./AdminSidebar.module.css";
import Button from "../../../../shared/ui/Button/Button.tsx";
import {useAuthStore} from "../../../../app/stores/authStore.ts";
import {authApi} from "../../../../app/api/authApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../../../shared/lib/errorHandler.ts";

interface IMenuItem {
    to: string;
    label: string;
    icon: React.ReactNode;
}

interface AdminSidebarProps {
    isOpen: boolean;
    onClose: () => void;
}

const AdminSidebar = ({ isOpen, onClose }: AdminSidebarProps) => {
    const location = useLocation();
    const navigate = useNavigate();

    const menuItems: IMenuItem[] = [
        {
            to: "/admin",
            label: "Dashboard",
            icon: <LayoutDashboard size={16} />
        },
        {
            to: "/admin/clothes",
            label: "Clothes",
            icon: <ShoppingBag size={16} />
        },
        {
            to: "/admin/orders",
            label: "Orders",
            icon: <Package size={16} /> },
        {
            to: "/admin/brands",
            label: "Brands",
            icon: <Tag size={16} />
        },
        {
            to: "/admin/collections",
            label: "Collections",
            icon: <Layers size={16} />
        },
        {
            to: "/admin/colors",
            label: "Colors",
            icon: <Palette size={16} />
        },
        {
            to: "/admin/tags",
            label: "Tags",
            icon: <Hash size={16} />
        },
        {
            to: "/admin/sizes",
            label: "Sizes",
            icon: <Ruler size={16} />
        },
        {
            to: "/admin/delivery",
            label: "Delivery",
            icon: <Truck size={16} />
        },
        {
            to: "/admin/reviews",
            label: "Reviews",
            icon: <MessageSquare size={16} />
        },
        {
            to: "/admin/questions",
            label: "Questions",
            icon: <HelpCircle size={16} />
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
        <aside className={`${styles.sidebar} ${isOpen ? styles.open : ""}`}>
            <div className={styles.header}>
                <div>
                    <h1 className={styles.logo}>Clothy</h1>
                    <p className={styles.subtitle}>Admin dashboard</p>
                </div>
                <button className={styles.closeButton} onClick={onClose}>
                    <X size={20} />
                </button>
            </div>

            <div className={styles.divider} />

            <nav className={styles.menu}>
                {menuItems.map((item) => (
                    <Link
                        key={item.to}
                        to={item.to}
                        className={`${styles.item} ${isActive(item.to) ? styles.active : ""}`}
                        onClick={onClose}
                    >
                        {item.icon}
                        <span>{item.label}</span>
                    </Link>
                ))}
            </nav>

            <div className={styles.footer}>
                <div className={styles.divider} />
                <div className={styles.footerInner}>
                    <Button
                        onClick={handleLogout}
                        variant="primary"
                        size="md"
                        icon={<LogOut size={18} />}
                    >
                        Logout
                    </Button>
                </div>
            </div>
        </aside>
    );
};

export default AdminSidebar;