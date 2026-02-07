import Container from '../../shared/Container/Container';
import styles from './AboutUsPage.module.css';
import { Helmet } from "react-helmet";
import PageWrapper from "../../shared/PageWrapper/PageWrapper.tsx";

const AboutUsPage = () => {
    return (
        <PageWrapper>
            <div>
                <Helmet>
                    <title>Clothy — About Us: Our Style and Story</title>
                    <meta
                        name="description"
                        content="Learn more about Clothy, our team, and brand history. Stylish clothing for modern people."
                    />
                </Helmet>

                <Container>
                    <section className={styles.storySection}>
                        <h2 className={styles.sectionTitle}>Our Story</h2>
                        <p>
                            <span className={styles.brandName}>Clothy</span> is a Ukrainian clothing store
                            offering items from various brands for men and women.
                            We help you choose stylish clothes for any occasion.
                        </p>

                        <p>
                            In our collection, you’ll find clothing that combines modern design with comfort.
                            We carefully select brands and models so every customer can find something for themselves.
                        </p>

                        <p>
                            Our team works every day to ensure you look stylish —
                            whether it’s a workday at the office or a weekend with friends.
                            We strive to make shopping enjoyable and convenient for everyone.
                        </p>
                    </section>

                    <section className={styles.statsSection}>
                        <div className={styles.statsContainer}>
                            <div className={styles.statCard}>
                                <div className={styles.statNumber}>2019</div>
                                <div className={styles.statLabel}>Year Founded</div>
                            </div>
                            <div className={styles.statCard}>
                                <div className={styles.statNumber}>15,000 +</div>
                                <div className={styles.statLabel}>Satisfied Customers</div>
                            </div>
                            <div className={styles.statCard}>
                                <div className={styles.statNumber}>500 +</div>
                                <div className={styles.statLabel}>Clothing Models</div>
                            </div>
                            <div className={styles.statCard}>
                                <div className={styles.statNumber}>100%</div>
                                <div className={styles.statLabel}>Quality Brands</div>
                            </div>
                        </div>
                    </section>
                </Container>
            </div>
        </PageWrapper>
    );
};

export default AboutUsPage;