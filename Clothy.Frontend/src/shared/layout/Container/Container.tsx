import type { ReactNode } from 'react';
import styles from './Container.module.css';

interface ContainerProps {
    children: ReactNode;
    className?: string;
    centered?: boolean;
    paddingY?: string | number;
    paddingX?: string | number;
}

const Container = ({ children, className = '', centered = true, paddingY, paddingX }: ContainerProps) => {
    const style: React.CSSProperties = {};
    if (paddingY !== undefined) {
        style.paddingTop = paddingY;
        style.paddingBottom = paddingY;
    }
    if (paddingX !== undefined) {
        style.paddingLeft = paddingX;
        style.paddingRight = paddingX;
    }

    return (
        <div
            className={`${styles.container} ${centered ? styles.centered : ''} ${className}`}
            style={Object.keys(style).length ? style : undefined}
        >
            {children}
        </div>
    );
};

export default Container;