import type {ReactNode} from "react";
import styles from "./AdminCard.module.css";

interface AdminCardProps {
    title?: ReactNode;
    actions?: ReactNode;
    /** no inner padding: for tables and lists that draw their own rows */
    flush?: boolean;
    className?: string;
    children: ReactNode;
    "aria-label"?: string;
}

const AdminCard = ({title, actions, flush = false, className = "", children, ...rest}: AdminCardProps) => (
    <section className={`${styles.card} ${className}`} aria-label={rest["aria-label"]}>
        {(title || actions) && (
            <div className={styles.header}>
                {title && <h2 className={styles.title}>{title}</h2>}
                {actions && <div className={styles.actions}>{actions}</div>}
            </div>
        )}
        <div className={`${styles.body} ${flush ? styles.flush : ""}`}>{children}</div>
    </section>
);

export default AdminCard;
