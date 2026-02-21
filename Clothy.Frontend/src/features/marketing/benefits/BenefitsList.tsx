import styles from "./BenefitsList.module.css";

interface IBenefitItem {
    title: string;
    description: string;
}

const benefits: IBenefitItem[] = [
    {
        title: "Free shipping",
        description: "Enjoy complimentary delivery on orders over ₴1500.",
    },
    {
        title: "Quality guarantee",
        description: "Carefully selected materials and verified authenticity.",
    },
    {
        title: "Welcome discount",
        description: "Get 10% off your first purchase.",
    },
];

const BenefitsList = () => {
    return (
        <section className={styles.wrapper}>
            <div className={styles.container}>
                {benefits.map(({title, description }) => (
                    <div className={styles.item} key={title}>
                        <div className={styles.textWrapper}>
                            <h2 className={styles.title}>{title}</h2>
                            <p className={styles.subtitle}>{description}</p>
                        </div>
                    </div>
                ))}
            </div>
        </section>
    );
};

export default BenefitsList;
