import styles from "./Loader.module.css";

const Loader = () => {
    return (
        <div
            className={styles.container}
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