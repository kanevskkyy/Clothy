import type { IBenefitItem } from "../../../pages/home/HomePage/HomePage";
import styles from "./BenefitsList.module.css";

interface BenefitsListProps {
    benefits: IBenefitItem[];
    titleSize?: string;
    subtitleSize?: string;
    uppercase?: boolean;
    marginBottom?: string;
}

const BenefitsList: React.FC<BenefitsListProps> = ({
                                                       benefits,
                                                       titleSize = '18px',
                                                       subtitleSize = '14px',
                                                       uppercase = false,
                                                       marginBottom,
                                                   }) => {
    return (
        <section className={styles.wrapper}>
            <div className={styles.container}>
                {benefits.map(({ title, description }) => (
                    <div className={styles.item} key={title}>
                        <div className={styles.textWrapper}>
                            <h2
                                className={styles.title}
                                style={{
                                    marginBottom,
                                    fontSize: titleSize,
                                    textTransform: uppercase ? 'uppercase' : 'initial',
                                }}
                            >
                                {title}
                            </h2>
                            <p
                                className={styles.subtitle}
                                style={{
                                    fontSize: subtitleSize,
                                    textTransform: uppercase ? 'uppercase' : 'initial',
                                }}
                            >
                                {description}
                            </p>
                        </div>
                    </div>
                ))}
            </div>
        </section>
    );
};

export default BenefitsList;
