import type { ReactNode } from 'react';
import styles from './Container.module.css';

interface ContainerProps {
    children: ReactNode;
    className?: string;
    centered?: boolean;
}

const Container = ({ children, className = '', centered = true }: ContainerProps) => {
    return (
        <div className={`${styles.container} ${centered ? styles.centered : ''} ${className}`}>
            {children}
        </div>
    );
};

export default Container;