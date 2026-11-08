import type {ReactNode} from "react";
import styles from "./AdminEmptyState.module.css";

interface AdminEmptyStateProps {
    icon?: ReactNode;
    title: string;
    description?: string;
    action?: ReactNode;
}

const AdminEmptyState = ({icon, title, description, action}: AdminEmptyStateProps) => (
    <div className={styles.empty} role="status">
        {icon && <div className={styles.icon} aria-hidden="true">{icon}</div>}
        <h2 className={styles.title}>{title}</h2>
        {description && <p className={styles.description}>{description}</p>}
        {action && <div className={styles.action}>{action}</div>}
    </div>
);

export default AdminEmptyState;
