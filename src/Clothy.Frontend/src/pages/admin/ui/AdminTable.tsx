import type {ReactNode} from "react";
import styles from "./AdminTable.module.css";

interface AdminTableProps {
    /** read by screen readers, not shown */
    caption: string;
    children: ReactNode;
}

/** Plain table with admin styles. Cell helpers live in AdminTable.module.css (num, muted, actions...). */
const AdminTable = ({caption, children}: AdminTableProps) => (
    <div className={styles.scroll}>
        <table className={styles.table}>
            <caption className={styles.srOnly}>{caption}</caption>
            {children}
        </table>
    </div>
);

export default AdminTable;
