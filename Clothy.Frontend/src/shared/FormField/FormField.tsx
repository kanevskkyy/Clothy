import type { ReactNode } from "react";
import styles from "./FormField.module.css";
import Label from "../Label/Label.tsx";
import ErrorMessage from "../ErrorMessage/ErrorMessage.tsx";

interface FormFieldProps {
    label: string;
    htmlFor: string;
    required?: boolean;
    error?: string;
    children: ReactNode;
}

const FormField = ({ label, htmlFor, required, error, children }: FormFieldProps) => {
    return (
        <div className={styles.field}>
            <Label htmlFor={htmlFor} required={required}>
                {label}
            </Label>
            {children}
            <ErrorMessage message={error} />
        </div>
    );
};

export default FormField;