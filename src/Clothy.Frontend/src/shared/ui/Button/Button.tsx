import { Link } from "react-router-dom";
import type { ReactNode, ButtonHTMLAttributes } from "react";
import styles from "./Button.module.css";

interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
    children: ReactNode;
    to?: string;
    icon?: ReactNode;
    variant?: "primary" | "secondary" | "outline" | "ghost";
    fullWidth?: boolean;
    size?: "sm" | "md" | "lg";
}

const Button = ({
                    children,
                    to,
                    onClick,
                    icon,
                    variant = "primary",
                    fullWidth = false,
                    size = "md",
                    disabled,
                    type = "button",
                    ...rest
                }: ButtonProps) => {
    const className = `${styles.button} ${styles[variant]} ${styles[size]} ${fullWidth ? styles.fullWidth : ''}`;

    if (to && !disabled) {
        return (
            <Link to={to} className={className}>
                <span>{children}</span>
                {icon}
            </Link>
        );
    }

    return (
        <button
            className={className}
            onClick={onClick}
            disabled={disabled}
            type={type}
            {...rest}
        >
            <span>{children}</span>
            {icon}
        </button>
    );
};

export default Button;