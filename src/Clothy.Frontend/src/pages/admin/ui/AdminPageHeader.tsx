import styles from "./AdminPageHeader.module.css";

interface AdminPageHeaderProps {
    title: string;
    description?: string;
}

const AdminPageHeader = ({title, description}: AdminPageHeaderProps) => (
    <div className={styles.header}>
        <h1 className={styles.title}>{title}</h1>
        {description && <p className={styles.description}>{description}</p>}
    </div>
);

export default AdminPageHeader;
