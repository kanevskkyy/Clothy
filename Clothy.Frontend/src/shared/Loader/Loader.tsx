import styles from "./Loader.module.css";

interface LoaderProps {
    marginTop?: React.CSSProperties['marginTop'];
}

const Loader = ({ marginTop, }: LoaderProps) => {
    return (
        <div
            className={styles.container}
            style={{ marginTop }}
            role="status"
            aria-live="polite"
        >
            <div className={styles.loader}>
                <div className={styles.loading}></div>
            </div>
            <p className={styles.loadingText}>
                Loading...
            </p>
        </div>
    );
};

export default Loader;