import styles from './AboutUsPage.module.css';
import {Helmet} from "react-helmet";
import type {IBenefitItem} from "../../home/HomePage/HomePage.tsx";
import BenefitsList from "../../../features/marketing/benefits/BenefitsList.tsx";
import Container from "../../../shared/layout/Container/Container.tsx";

interface IOurValues {
    valueNumber: string;
    valueDescription: string;
}

const AboutUsPage = () => {
    const ourValuesList: IOurValues[] = [
        {
            valueNumber: "01",
            valueDescription: "Quality over quantity — we select fewer pieces, but each is crafted to stand the test of time."
        },
        {
            valueNumber: "02",
            valueDescription: "Responsibility — we prioritize conscious production, sustainable materials, and long-term impact."
        },
        {
            valueNumber: "03",
            valueDescription: "Inclusivity — clothing designed to celebrate individuality, confidence, and every unique story."
        },
    ];

    const benefits: IBenefitItem[] = [
        {
            title: "50+",
            description: "Premium Brands",
        },
        {
            title: "10K+",
            description: "Happy Customers",
        },
        {
            title: "30+",
            description: "Countries Shipped",
        },
    ];

    return (
        <Container>
            <div>
                <Helmet>
                    <title>Clothy — About Us: Our Style and Story</title>
                    <meta
                        name="description"
                        content="Learn more about Clothy, our team, and brand history. Stylish clothing for modern people."
                    />
                </Helmet>

                <section className={styles.aboutUsSection}>
                    <h2 className={styles.aboutUsTitle}>About Us</h2>
                    <p className={styles.aboutUsDescription}>We believe fashion should be accessible, sustainable, and
                        unique — just like <br/> you.</p>
                </section>


                {/*//TODO: Here rename if will rename full project*/}

                <section className={styles.ourStory}>
                    <img src="src/assets/images/aboutUsBanner.jpg"
                         alt="About us banner"
                         loading="lazy"/>

                    <div className={styles.textWrapper}>
                        <h2 className={styles.storyTitle}>Our story</h2>

                        <p className={styles.ourStoryDescription}>
                            Founded in 2020, Clothy began with a clear vision — to create clothing that feels as refined as
                            it looks.
                            We carefully curate each collection, partnering with established labels and emerging designers
                            who
                            share our commitment to craftsmanship and intentional design.
                        </p>

                        <p className={styles.ourStoryDescription}>
                            What started as a single space has grown into a global destination for those who value quality
                            materials,
                            considered silhouettes, and timeless aesthetics. Every piece we offer is selected with purpose —
                            made to last,
                            made to be worn, and made to matter.
                        </p>
                    </div>
                </section>

                <BenefitsList
                    benefits={benefits}
                    titleSize="36px"
                    subtitleSize="14px"
                    uppercase
                    marginBottom="8px"
                />

                <section className={styles.ourValues}>
                    <h2 className={styles.ourValuesTitle}>Our Values</h2>
                    <ul className={styles.ourValuesList}>
                        {ourValuesList.map(({valueNumber, valueDescription}) => (
                            <li key={valueNumber} className={styles.ourValuesItem}>
                                <span className={styles.valueNumber}>{valueNumber}</span>
                                <p className={styles.valueDescription}>{valueDescription}</p>
                            </li>
                        ))}
                    </ul>
                </section>

                <section className={styles.contactUs}>
                    <h2 className={styles.contactUsTitle}>Contact Us</h2>
                    <p className={styles.contactUsText}>
                        Have questions? Reach out at{' '}
                        <a href="mailto:info@atelier.com" className={styles.contactLink}>info@atelier.com</a>
                        {' '}or call us at{' '}
                        <a href="tel:+18001234567" className={styles.contactLink}>+1 (800) 123-4567</a>.
                    </p>
                </section>
            </div>
        </Container>
    );
};

export default AboutUsPage;