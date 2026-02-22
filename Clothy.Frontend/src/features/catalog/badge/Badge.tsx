import React from 'react';
import styles from "./Badge.module.css";

interface IBadgeProps {
    label: string;
    icon?: React.ReactNode;
    fontSize?: string;
    color?: string;
    background?: string;
    borderColor?: string;
}

const Badge: React.FC<IBadgeProps> = ({
                                          label,
                                          icon,
                                          fontSize = "12px",
                                          color = "#F6A821",
                                          background = "#FEF1DB",
                                          borderColor = "#FCE2B7",
                                      }) => {
    return (
        <div
            className={styles.badge}
            style={{ fontSize, color, background, borderColor }}
        >
            {icon && <span className={styles.icon}>{icon}</span>}
            {label}
        </div>
    );
};

export default Badge;