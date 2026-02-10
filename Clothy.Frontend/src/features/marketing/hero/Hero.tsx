import styles from './Hero.module.css';
import { ArrowRight } from "lucide-react";
import Button from '../../../shared/Button/Button.tsx';

const Hero = () => {
    return (
        <section className={styles.hero}>
            <div className={styles.backgroundWrapper}>
                <img
                    src="src/assets/images/banner.png"
                    alt="Fashion model"
                    className={styles.backgroundImage}
                />
                <div className={styles.overlay} />
            </div>

            <div className={styles.container}>
                <div className={styles.content}>
                    <h1 className={styles.title}>
                        Style that
                        <br />
                        defines you
                    </h1>
                    <p className={styles.description}>
                        Discover the best brands and latest collections. Your perfect look starts here.
                    </p>
                    <div className={styles.buttons}>
                        <Button
                            to="/catalog"
                            variant="secondary"
                            icon={<ArrowRight size={20} />}
                        >
                            Go to catalog
                        </Button>
                    </div>
                </div>
            </div>
        </section>
    );
};

export default Hero;