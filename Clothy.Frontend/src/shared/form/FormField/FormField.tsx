import type { ReactNode } from "react";
import styles from "./FormField.module.css";
import Label from "../../ui/Label/Label.tsx";
import ErrorMessage from "../ErrorMessage/ErrorMessage.tsx";

interface FormFieldProps {
    label: string;
    htmlFor: string;
    required?: boolean;
    flexDirection?: "row" | "column";
    error?: string;
    children: ReactNode;
}

const FormField = ({
                       label,
                       htmlFor,
                       required,
                       error,
                       children,
                       flexDirection = "column"
                   }: FormFieldProps) => {
    return (
        <div
            className={styles.field}
            style={{
                flexDirection,
                ...(flexDirection === "row" ? { alignItems: "center" } : {})
            }}
        >
            <Label htmlFor={htmlFor} required={required}>
                {label}
            </Label>
            {children}
            <ErrorMessage message={error} />
        </div>
    );
};

export default FormField;