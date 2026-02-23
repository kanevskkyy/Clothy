import styles from './Hero.module.css';
import Button from "../../../shared/ui/Button/Button.tsx";
import {ArrowRight} from "lucide-react";

const Hero = () => {
    return (
        <section className={styles.hero}>
            <div className={styles.content}>
                <p className={styles.collection}>Spring — Summer 2026 Collection</p>
                <h2 className={styles.title}>
                    The Art of <br/>
                    <span className={styles.accent}>Being You</span>
                </h2>
                <p className={styles.description}>Clothing that highlights your individuality. Crafted with love for
                    every detail.</p>
                <div className={styles.buttonWrapper}>
                    <Button
                        to="/catalog"
                        variant="primary"
                        icon={<ArrowRight size={20}/>}
                    >
                        Browse Catalog
                    </Button>
                </div>
            </div>
            <div className={styles.imageWrapper}>
                <img
                    src="src/assets/images/banner.png"
                    alt="Hero image"
                    className={styles.image}
                    loading="lazy"
                />
            </div>
        </section>
    );
};

export default Hero;