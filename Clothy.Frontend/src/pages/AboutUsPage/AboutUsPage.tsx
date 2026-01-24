import Container from '../../shared/Container/Container';
import styles from './AboutUsPage.module.css';

const AboutUsPage = () => {
    return (
        <div>
            <Container>
                <section className={styles.storySection}>
                    <h2 className={styles.sectionTitle}>Наша історія</h2>
                    <p>
                        <span className={styles.brandName}>Clothy</span> — це український магазин одягу,
                        який пропонує речі від різних брендів для чоловіків та жінок.
                        Ми допомагаємо обирати стильний одяг для будь-якого випадку.
                    </p>

                    <p>
                        У нашому асортименті ви знайдете одяг, який поєднує сучасний дизайн з комфортом у носінні.
                        Ми ретельно обираємо бренди та моделі, щоб кожен наш клієнт міг знайти щось для себе.
                    </p>

                    <p>
                        Наша команда щодня працює над тим, щоб ви могли виглядати стильно —
                        чи це робочий день в офісі, чи вихідні з друзями.
                        Ми прагнемо робити покупки приємними та зручними для кожного.
                    </p>
                </section>

                <section className={styles.statsSection}>
                    <div className={styles.statsContainer}>
                        <div className={styles.statCard}>
                            <div className={styles.statNumber}>2019</div>
                            <div className={styles.statLabel}>Рік заснування</div>
                        </div>
                        <div className={styles.statCard}>
                            <div className={styles.statNumber}>15000 +</div>
                            <div className={styles.statLabel}>Задоволених клієнтів</div>
                        </div>
                        <div className={styles.statCard}>
                            <div className={styles.statNumber}>500 +</div>
                            <div className={styles.statLabel}>Моделей одягу</div>
                        </div>
                        <div className={styles.statCard}>
                            <div className={styles.statNumber}>100 %</div>
                            <div className={styles.statLabel}>Якісні бренди</div>
                        </div>
                    </div>
                </section>
            </Container>
        </div>
    );
};

export default AboutUsPage;