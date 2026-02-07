import type { InputHTMLAttributes } from "react";
import styles from "./Input.module.css";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
    error?: boolean;
}

const Input = ({ error, className, ...rest }: InputProps) => {
    return (
        <input
            className={`${styles.input} ${error ? styles.error : ""} ${className || ""}`}
            {...rest}
        />
    );
};

export default Input;