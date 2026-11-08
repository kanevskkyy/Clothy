import type {ReactNode} from "react";
import styles from "./AdminToolbar.module.css";

interface AdminToolbarProps {
    start?: ReactNode;
    end?: ReactNode;
}

const AdminToolbar = ({start, end}: AdminToolbarProps) => (
    <div className={styles.toolbar}>
        {start && <div className={styles.start}>{start}</div>}
        {end && <div className={styles.end}>{end}</div>}
    </div>
);

export default AdminToolbar;
