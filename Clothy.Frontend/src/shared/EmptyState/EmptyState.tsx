import Button from "../Button/Button";
import styles from "./EmptyState.module.css";
import type {ReactNode} from "react";

interface EmptyStateButton {
    label: string;
    to?: string;
    onClick?: () => void;
    variant?: "primary" | "secondary" | "outline" | "ghost";
    size?: "sm" | "md" | "lg";
    fullWidth?: boolean;
}

interface EmptyStateProps {
    icon?: ReactNode;
    title: string;
    description?: string;
    buttons?: EmptyStateButton[];
}

const EmptyState = ({ icon, title, description, buttons }: EmptyStateProps) => {
    return (
        <div className={styles.wrapper}>
            <div className={styles.container}>
                {icon && <div className={styles.mainIcon}>{icon}</div>}
                <h1>{title}</h1>
                {description && <p>{description}</p>}
                {buttons?.map((btn, idx) => (
                    <Button
                        key={idx}
                        to={btn.to}
                        onClick={btn.onClick}
                        variant={btn.variant}
                        size={btn.size}
                        fullWidth={btn.fullWidth}
                    >
                        {btn.label}
                    </Button>
                ))}
            </div>
        </div>
    );
};

export default EmptyState;