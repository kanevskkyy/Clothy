import {Link} from "react-router-dom";
import {ArrowRight} from "lucide-react";
import styles from "./SaleBanner.module.css";

const SaleBanner = () => {
    return (
        <section className={styles.section}>
            <div className={styles.banner}>
                <div className={styles.content}>
                    <span className={styles.badge}>Розпродаж</span>
                    <h2 className={styles.title}>
                        До -50% на вибрані товари
                    </h2>
                    <p className={styles.description}>
                        Не пропустіть можливість оновити свій гардероб за вигідними цінами!
                    </p>
                    <Link to="/catalog" className={styles.button}>
                        До каталогу
                        <ArrowRight className={styles.icon}/>
                    </Link>
                </div>
                <div className={styles.gradientOverlay}/>
            </div>
        </section>
    );
};

export default SaleBanner;