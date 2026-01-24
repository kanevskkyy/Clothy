import { Link } from "react-router-dom";
import type { ReactNode } from "react";
import styles from "./Button.module.css";

interface ButtonProps {
    children: ReactNode;
    to?: string;
    onClick?: () => void;
    icon?: ReactNode;
    variant?: "primary" | "secondary";
}

const Button = ({
                    children,
                    to,
                    onClick,
                    icon,
                    variant = "primary",
                }: ButtonProps) => {
    const className = `${styles.button} ${styles[variant]}`;

    if (to) {
        return (
            <Link to={to} className={className}>
                <span>{children}</span>
                {icon}
            </Link>
        );
    }

    return (
        <button className={className} onClick={onClick}>
            <span>{children}</span>
            {icon}
        </button>
    );
};

export default Button;