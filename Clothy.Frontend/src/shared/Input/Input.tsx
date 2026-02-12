import { forwardRef, type InputHTMLAttributes } from "react";
import styles from "./Input.module.css";

interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
    error?: boolean;
}

const Input = forwardRef<HTMLInputElement, InputProps>(
    ({ error, className, ...rest }, ref) => {
        return (
            <input
                ref={ref}
                className={`${styles.input} ${error ? styles.error : ""} ${className || ""}`}
                {...rest}
            />
        );
    }
);

Input.displayName = "Input";

export default Input;