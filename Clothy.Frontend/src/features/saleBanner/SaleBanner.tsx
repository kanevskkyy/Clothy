import {ArrowRight} from "lucide-react";
import styles from "./SaleBanner.module.css";
import Button from "../../shared/Button/Button.tsx";

const SaleBanner = () => {
    return (
        <section className={styles.section}>
            <div className={styles.banner}>
                <div className={styles.content}>
                    <h2 className={styles.title}>
                        До -50% на вибрані товари
                    </h2>
                    <p className={styles.description}>
                        Не пропустіть можливість оновити свій гардероб за вигідними цінами!
                    </p>
                    <Button
                        to="/catalog"
                        icon={<ArrowRight size={20} />}
                    >
                        Перейти до каталогу
                    </Button>
                </div>
                <div className={styles.gradientOverlay}/>
            </div>
        </section>
    );
};

export default SaleBanner;