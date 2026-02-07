import { memo } from "react";
import { ChevronDown } from "lucide-react";
import styles from "./SortSelect.module.css";

export interface SortOption {
    value: string;
    label: string;
}

interface SortSelectProps {
    value: string;
    options: SortOption[];
    onChange: (value: string) => void;
    label?: string;
}

const SortSelect = memo(({ value, options = [], onChange, label = "Сортування:" }: SortSelectProps) => {
    if (!options || options.length === 0) {
        return null;
    }

    return (
        <div className={styles.sortContainer}>
            <label className={styles.sortLabel}>{label}</label>
            <div className={styles.selectWrapper}>
                <select
                    className={styles.select}
                    value={value}
                    onChange={(e) => onChange(e.target.value)}
                >
                    {options.map(option => (
                        <option key={option.value} value={option.value}>
                            {option.label}
                        </option>
                    ))}
                </select>
                <ChevronDown className={styles.selectIcon} size={18} />
            </div>
        </div>
    );
});

export default SortSelect;