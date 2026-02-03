import styles from './Hero.module.css';
import { ArrowRight } from "lucide-react";
import Button from '../../shared/Button/Button';

const Hero = () => {
    return (
        <section className={styles.hero}>
            <div className={styles.backgroundWrapper}>
                <img
                    src="../../../src/assets/images/banner.png"
                    alt="Fashion model"
                    className={styles.backgroundImage}
                />
                <div className={styles.overlay} />
            </div>

            <div className={styles.container}>
                <div className={styles.content}>
                    <h1 className={styles.title}>
                        Стиль, який
                        <br />
                        підкреслює вас
                    </h1>
                    <p className={styles.description}>
                        Відкрийте для себе найкращі бренди та актуальні колекції. Ваш ідеальний образ починається тут.
                    </p>
                    <div className={styles.buttons}>
                        <Button
                            to="/catalog"
                            icon={<ArrowRight size={20} />}
                        >
                            Перейти до каталогу
                        </Button>
                    </div>
                </div>
            </div>
        </section>
    );
};

export default Hero;