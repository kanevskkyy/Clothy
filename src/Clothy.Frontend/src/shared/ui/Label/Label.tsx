import type { LabelHTMLAttributes, ReactNode } from "react";
import styles from "./ Label.module.css";

interface LabelProps extends LabelHTMLAttributes<HTMLLabelElement> {
    children: ReactNode;
    required?: boolean;
}

const Label = ({ children, required, ...rest }: LabelProps) => {
    return (
        <label className={styles.label} {...rest}>
            {children}
            {required && "*"}
        </label>
    );
};

export default Label;