import { ArrowRight } from "lucide-react";
import styles from "./SaleBanner.module.css";
import Button from "../../shared/Button/Button.tsx";

const SaleBanner = () => {
    return (
        <section className={styles.section}>
            <div className={styles.banner}>
                <div className={styles.content}>
                    <h2 className={styles.title}>
                        Up to 50% off selected items
                    </h2>
                    <p className={styles.description}>
                        Don’t miss the chance to refresh your wardrobe at great prices!
                    </p>
                    <Button
                        to="/catalog"
                        variant="secondary"
                        icon={<ArrowRight size={20} />}
                    >
                        Go to catalog
                    </Button>
                </div>
                <div className={styles.gradientOverlay} />
            </div>
        </section>
    );
};

export default SaleBanner;