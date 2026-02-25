import {Link, useLocation, useNavigate} from "react-router-dom";
import {Package, Star, User, LogOut} from "lucide-react";
import styles from "./AccountSidebar.module.css";
import {authApi} from "../../../../app/api/authApi.ts";
import {toast} from "sonner";
import {getErrorMessage} from "../../../../shared/lib/errorHandler.ts";
import Button from "../../../../shared/ui/Button/Button.tsx";
import {useAuthStore} from "../../../../app/api/stores/authStore.ts";

const AccountSidebar = () => {
    const location = useLocation();
    const navigate = useNavigate();

    const menuItems = [
        {
            to: "/account",
            icon: <User size={16}/>,
            label: "Profile"
        },
        {
            to: "/account/orders",
            icon: <Package size={16}/>,
            label: "Orders"
        },
        {
            to: "/account/reviews",
            icon: <Star size={16}/>,
            label: "Reviews"
        },
    ];

    const isActive = (path: string) => {
        if (path === "/account") return location.pathname === "/account";
        return location.pathname === path;
    };

    const handleLogout = () => {
        useAuthStore.getState().logout();
        navigate("/login", { replace: true });
        authApi.logoutAsync().catch((error) => {
            toast.error(getErrorMessage(error));
        });
    };

    return (
        <div className={styles.wrapper}>
            <nav className={styles.tabs}>
                {menuItems.map((item) => (
                    <Link
                        key={item.to}
                        to={item.to}
                        className={`${styles.tab} ${isActive(item.to) ? styles.active : ""}`}
                    >
                        {item.icon}
                        <span>{item.label}</span>
                    </Link>
                ))}
            </nav>
            <Button
                onClick={handleLogout}
                variant="outline"
                size="sm"
                icon={<LogOut size={18} />}
            >
                Logout
            </Button>
        </div>
    );
};

export default AccountSidebar;