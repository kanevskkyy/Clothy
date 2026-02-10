import type { TextareaHTMLAttributes } from "react";
import styles from "./Textarea.module.css";

interface TextareaProps extends TextareaHTMLAttributes<HTMLTextAreaElement> {
    error?: boolean;
}

const Textarea = ({ error, className, ...rest }: TextareaProps) => {
    return (
        <textarea
            className={`${styles.textarea} ${error ? styles.error : ""} ${className || ""}`}
            {...rest}
        />
    );
};

export default Textarea;