import styles from "./Loader.module.css";
import Container from "../../layout/Container/Container.tsx";

const Loader = () => {
    return (
        <Container>
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
        </Container>
    );
};

export default Loader;