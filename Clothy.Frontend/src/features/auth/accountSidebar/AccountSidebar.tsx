import { Link, useLocation } from "react-router-dom";
import { User, Package, Star } from "lucide-react";
import styles from "./AccountSidebar.module.css";

const AccountSidebar = () => {
    const location = useLocation();

    const menuItems = [
        {
            to: "/account",
            icon: <User size={20} />,
            label: "Account",
        },
        {
            to: "/account/orders",
            icon: <Package size={20} />,
            label: "Orders",
        },
        {
            to: "/account/reviews",
            icon: <Star size={20} />,
            label: "Reviews",
        },
    ];

    const isActive = (path: string) => {
        if (path === "/account") {
            return location.pathname === "/account";
        }
        return location.pathname === path;
    };

    return (
        <nav className={styles.sidebar}>
            {menuItems.map((item) => (
                <Link
                    key={item.to}
                    to={item.to}
                    className={`${styles.link} ${isActive(item.to) ? styles.active : ""}`}
                >
                    {item.icon}
                    <span>{item.label}</span>
                </Link>
            ))}
        </nav>
    );
};

export default AccountSidebar;