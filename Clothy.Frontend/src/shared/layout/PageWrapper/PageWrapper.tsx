import type { ReactNode } from 'react';
import styles from './PageWrapper.module.css';

interface PageWrapperProps {
    children: ReactNode;
    className?: string;
}

const PageWrapper = ({ children, className = '' }: PageWrapperProps) => {
    return (
        <div className={`${styles.wrapper} ${className}`}>
            {children}
        </div>
    );
};

export default PageWrapper;
