import { memo } from "react";
import styles from "./Checkbox.module.css";

interface CheckboxProps {
    id: string;
    label: string;
    count?: number;
    checked: boolean;
    onChange: (checked: boolean) => void;
}

const Checkbox = memo(({ id, label, count, checked, onChange }: CheckboxProps) => {
    return (
        <label className={styles.checkboxItem}>
            <input
                type="checkbox"
                id={id}
                checked={checked}
                onChange={(e) => onChange(e.target.checked)}
                className={styles.checkbox}
            />
            <span className={styles.checkboxLabel}>
                {label}
                {count !== undefined && (
                    <span className={styles.itemCount}>({count})</span>
                )}
            </span>
        </label>
    );
});

export default Checkbox;
