import type { ReactNode } from "react";
import styles from "./RadioOption.module.css";

interface RadioOptionProps {
    id: string;
    name: string;
    value: string;
    checked: boolean;
    onChange: (value: string) => void;
    icon?: ReactNode;
    iconBgColor?: string;
    iconColor?: string;
    label: string;
    description?: string;
}

const RadioOption = ({
                         id,
                         name,
                         value,
                         checked,
                         onChange,
                         icon,
                         iconBgColor,
                         iconColor,
                         label,
                         description
                     }: RadioOptionProps) => {
    return (
        <label
            htmlFor={id}
            className={`${styles.radioOption} ${checked ? styles.checked : ''}`}
        >
            <input
                type="radio"
                id={id}
                name={name}
                value={value}
                checked={checked}
                onChange={(e) => onChange(e.target.value)}
                className={styles.radioInput}
            />
            <div className={styles.radioContent}>
                {icon && (
                    <div
                        className={styles.icon}
                        style={{
                            backgroundColor: iconBgColor,
                            color: iconColor
                        }}
                    >
                        {icon}
                    </div>
                )}
                <div className={styles.textContent}>
                    <div className={styles.label}>{label}</div>
                    {description && <div className={styles.description}>{description}</div>}
                </div>
            </div>
        </label>
    );
};

export default RadioOption;