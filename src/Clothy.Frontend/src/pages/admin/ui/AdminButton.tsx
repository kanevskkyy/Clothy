import type {ButtonHTMLAttributes, ReactNode} from "react";
import styles from "./AdminButton.module.css";

interface AdminButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    variant?: "primary" | "secondary" | "quiet" | "danger";
    size?: "sm" | "md";
    icon?: ReactNode;
}

/** Compact button for tables and toolbars. With no children, pass aria-label. */
const AdminButton = ({variant = "secondary", size = "sm", icon, children, className = "", type = "button", ...rest}: AdminButtonProps) => (
    <button
        type={type}
        className={`${styles.button} ${styles[variant]} ${styles[size]} ${children ? "" : styles.iconOnly} ${className}`}
        {...rest}
    >
        {icon && <span aria-hidden="true" style={{display: "inline-flex"}}>{icon}</span>}
        {children}
    </button>
);

export default AdminButton;
