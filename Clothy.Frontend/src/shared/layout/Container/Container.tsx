import type { ReactNode } from 'react';
import styles from './Container.module.css';

interface ContainerProps {
    children: ReactNode;
    className?: string;
    centered?: boolean;
    paddingY?: string | number;
}

const Container = ({ children, className = '', centered = true, paddingY }: ContainerProps) => {
    return (
        <div
            className={`${styles.container} ${centered ? styles.centered : ''} ${className}`}
            style={paddingY !== undefined ? { paddingTop: paddingY, paddingBottom: paddingY } : undefined}
        >
            {children}
        </div>
    );
};

export default Container;