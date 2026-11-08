import type {ReactNode} from "react";
import styles from "./AdminBadge.module.css";

export type AdminBadgeTone = "neutral" | "success" | "warning" | "danger" | "info";

interface AdminBadgeProps {
    tone?: AdminBadgeTone;
    /** decorative: the text carries the meaning */
    icon?: ReactNode;
    title?: string;
    children: ReactNode;
}

const AdminBadge = ({tone = "neutral", icon, title, children}: AdminBadgeProps) => (
    <span className={`${styles.badge} ${styles[tone]}`} title={title}>
        {icon && <span aria-hidden="true" style={{display: "inline-flex"}}>{icon}</span>}
        {children}
    </span>
);

export default AdminBadge;
